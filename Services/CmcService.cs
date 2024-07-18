
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;

using JellyfishTool.Config;
using JellyfishTool.Models;
using JellyfishTool.Models.DTO;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace JellyfishTool.Services {

    public class CmcService {

        private static readonly string ROUTE_REQUEST_CMC = "api2/PKI-v1/HandleCMCRequest";

        private static readonly string PKI_REQUEST_OID = "1.3.6.1.5.5.7.12.2";
        private static readonly string PKI_RESPONSE_OID = "1.3.6.1.5.5.7.12.3";
        private static readonly string SHA_ALGO_OID = "1.3.14.3.2.26";

        private static readonly string CMC_ADD_ATTRIBUTED_OID = "1.3.6.1.4.1.311.10.10.1";
        private static readonly string CMC_CLIENT_INFO_OID = "1.3.6.1.4.1.311.21.20";
        private static readonly string CMC_DATA_OID = "1.2.840.113549.1.7.1";
        private static readonly string CMC_REVOCATION_OID = "1.3.6.1.5.5.7.7.17";

        public static readonly int CMC_STATUS_INFO_SUCCESS = 0;
        public static readonly int CMC_STATUS_INFO_FAILED = 2;

        private static readonly string CMC_PEM_HEADER_TYPE = "NEW CERTIFICATE REQUEST";

        private static readonly int JF_REQUEST_CLIENT_ID = 74;     //ASCII 'J' character in hex (in decimal)

        private readonly OpenJellyfishToolSettings settings;
        private readonly SessionService session;
        private readonly ClientService clientFactory;

        public CmcService(
            OpenJellyfishToolSettings settings,
            SessionService session,
            ClientService clientFactory
        ) {
            this.settings = settings;
            this.session = session;
            this.clientFactory = clientFactory;
        }

        public async Task<RequestCmcResponse> RequestCertificate(
            int caId,
            int licensedTemplateId,
            string cmcRequest
        ) {
            
            Console.WriteLine("Submitting CMC certificate request...");
            return await SubmitRequest(caId, cmcRequest, licensedTemplateId);
        }

        public async Task<RequestCmcResponse> RequestRevoke(
            int caId,
            string cmcRequest
        ) {
            
            Console.WriteLine("Submitting CMC certificate revocation request...");
            return await SubmitRequest(caId, cmcRequest);
        }

        public async Task<RequestCmcResponse> SubmitRequest(
            int caId,
            string cmcRequest,
            int? licensedTemplateId = null
        ) {

            //Prepare request
            string address = $"{settings.Jellyfish.Address}/{ROUTE_REQUEST_CMC}";
            RequestCmcRequest model = new RequestCmcRequest() {
                CaId = caId,
                Cmc = cmcRequest
            };

            if (licensedTemplateId != null) {
                model.LicensedTemplateId = (int)licensedTemplateId;
            }

            JsonContent payload = JsonContent.Create(model);

            //Prepare client
            using HttpClient client = clientFactory.GetAuthenticatedClient();

            //Submit
            Console.WriteLine($"Submitting post: {address}");
            HttpResponseMessage response = await client.PostAsync(address, payload);
            if (!response.IsSuccessStatusCode) {
                throw new InvalidOperationException($"Post submit cmc request response not success: {response.ReasonPhrase}");
            }

            //Read
            string cmcResponse = await response.Content.ReadAsStringAsync();
            RequestCmcResponse cmc = JsonSerializer.Deserialize<RequestCmcResponse>(cmcResponse);

            return cmc;
        }

        public string ConvertPkcs10ToCmc(string csr) {

            Console.WriteLine("Transmuting PKCS10 to CMC...");

            //Load csr
            byte[] csrContent = CryptoService.PemDecode(csr);

            //Generate cmc
            using MemoryStream reqStream = new MemoryStream();
            using (DerSequenceGenerator reqSequence = new DerSequenceGenerator(reqStream, 0, false)) {
                reqSequence.AddObject(new DerInteger(1));
                reqSequence.AddObject(Asn1Object.FromByteArray(csrContent));
            }

            Asn1EncodableVector pkiContent = new Asn1EncodableVector() {
                new DerSequence(    //Control sequence
                    new DerSequence(
                        new DerInteger(2),
                        new DerObjectIdentifier(CMC_ADD_ATTRIBUTED_OID),
                        new DerSet(     //CMC attributes
                            new DerSequence(
                                new DerInteger(0),
                                new DerSequence(
                                    new DerInteger(1)
                                ),
                                new DerSet(
                                    new DerSequence(
                                        new DerObjectIdentifier(CMC_CLIENT_INFO_OID),
                                        new DerSet(     //Client info
                                            new DerSequence(
                                                new DerInteger(JF_REQUEST_CLIENT_ID),
                                                new DerUtf8String(IdentityService.GetMachineName()),                            //Machine name
                                                new DerUtf8String(IdentityService.HashApiKey(settings.Jellyfish.Auth.ApiKey)),  //User API key
                                                new DerUtf8String(IdentityService.GetProductName())                             //Product name
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                ),
                new DerSequence(    //Req sequence
                    Asn1Object.FromByteArray(reqStream.ToArray())
                ),
                new DerSequence(),  //Cms sequence
                new DerSequence()   //Other message sequence
            };
            
            Asn1Object pkiData = new DerSequence(pkiContent).ToAsn1Object();
            CmsProcessable pkiDataCms = new CmsProcessableByteArray(pkiData.GetDerEncoded());

            //Load signer
            X509Certificate2 signerCert = session.SignerCertificate;
            AsymmetricAlgorithm signerKey = session.SignerKey;

            if (signerCert == null || signerKey == null) {
                throw new InvalidOperationException("Signer or key not specified, PKCS7 data cannot be signed");
            }

            //Create pkcs7 generator
            CmsSignedDataGenerator generator = new CmsSignedDataGenerator();
            Org.BouncyCastle.X509.X509Certificate bcSigner = DotNetUtilities.FromX509Certificate(signerCert); 
            AsymmetricCipherKeyPair bcKey = DotNetUtilities.GetKeyPair(signerKey);
            generator.AddCertificate(bcSigner);
            generator.AddSigner(bcKey.Private, bcSigner, SHA_ALGO_OID);

            CmsSignedData p7 = generator.Generate(PKI_REQUEST_OID, pkiDataCms, true);

            //Convert to pem
            byte[] p7Bytes = p7.GetEncoded();
            string pem = CryptoService.PemEncode(CMC_PEM_HEADER_TYPE, p7Bytes);
                        
            return pem;
        }

        public string CreateCmcRevocation(string issuerSubject, string serialString, RevocationReason revocationReason, string comment) {

            //Parse issuer subject
            X509Name issuer = new X509Name(issuerSubject);

            //Parse hex encoded string and convert to decimal
            BigInteger serialNumber;
            try {
                serialNumber = new BigInteger(serialString, 16);
            } catch (Exception ex) {

                //Handle bad serialString format
                throw new ArgumentException($"Serial number is not hex: {ex.Message}");
            }

            //Create revocation request
            Asn1EncodableVector revocationContent = new Asn1EncodableVector() {
                new DerSequence(    //Control sequence
                    new DerSequence(
                        new DerInteger(2),
                        new DerObjectIdentifier(CMC_ADD_ATTRIBUTED_OID),
                        new DerSet(     
                            new DerSequence(    //CMC attributes
                                new DerInteger(0),
                                new DerSequence(
                                    new DerInteger(1)
                                ),
                                new DerSet(
                                    new DerSequence(
                                        new DerObjectIdentifier(CMC_CLIENT_INFO_OID),
                                        new DerSet(     
                                            new DerSequence(    //Client info
                                                new DerInteger(JF_REQUEST_CLIENT_ID),
                                                new DerUtf8String(IdentityService.GetMachineName()),                            //Machine name
                                                new DerUtf8String(IdentityService.HashApiKey(settings.Jellyfish.Auth.ApiKey)),  //User API key
                                                new DerUtf8String(IdentityService.GetProductName())                             //Product name
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    ),
                    new DerSequence(
                        new DerInteger(2),
                        new DerObjectIdentifier(CMC_REVOCATION_OID),
                        new DerSet(
                            new DerSequence(    //Revocation control
                                issuer.ToAsn1Object(),                      //Issuer subject
                                new DerInteger(serialNumber),               //Serial number as an integer
                                new DerEnumerated((int)revocationReason),   //Revocation reason
                                new DerUtf8String(comment)                  //Comment, optional but I'm always including
                            )
                        )
                    )
                ),
                new DerSequence(),  //Req sequence
                new DerSequence(),  //Cms sequence
                new DerSequence()   //Other message sequence
            };

            Asn1Object revocationData = new DerSequence(revocationContent).ToAsn1Object();
            CmsProcessable revocationDataCms = new CmsProcessableByteArray(revocationData.GetDerEncoded());

            //Load signer
            X509Certificate2 signerCert = session.SignerCertificate;
            AsymmetricAlgorithm signerKey = session.SignerKey;

            //Create pkcs7 generator
            CmsSignedDataGenerator generator = new CmsSignedDataGenerator();
            Org.BouncyCastle.X509.X509Certificate bcSigner = DotNetUtilities.FromX509Certificate(signerCert); 
            AsymmetricCipherKeyPair bcKey = DotNetUtilities.GetKeyPair(signerKey);
            generator.AddCertificate(bcSigner);
            generator.AddSigner(bcKey.Private, bcSigner, SHA_ALGO_OID);

            CmsSignedData p7 = generator.Generate(PKI_REQUEST_OID, revocationDataCms, true);

            //Convert to pem
            byte[] p7Bytes = p7.GetEncoded();
            string pem = CryptoService.PemEncode(CMC_PEM_HEADER_TYPE, p7Bytes);
                        
            return pem;
        }

        public static Tuple<int, string> ReadCmcStatus(byte[] p7Bytes) {
            
            //Read all the wrapper
            Asn1Sequence p7 = Asn1Sequence.GetInstance(p7Bytes);
            Asn1TaggedObject taggedContainer = Asn1TaggedObject.GetInstance(p7.ElementAt(1));
            Asn1Sequence container = Asn1Sequence.GetInstance(taggedContainer.GetExplicitBaseObject());

            Asn1Sequence pkiResponse = Asn1Sequence.GetInstance(container.ElementAt(2));
            DerObjectIdentifier contentType = DerObjectIdentifier.GetInstance(pkiResponse.ElementAt(0));

            //Return if this is not a response
            if (contentType.Id != PKI_RESPONSE_OID) {
                return null;
            }

            //Unwrap down to the status info
            Asn1TaggedObject taggedResponseOctet = Asn1TaggedObject.GetInstance(pkiResponse.ElementAt(1));
            Asn1OctetString responseOctet = Asn1OctetString.GetInstance(taggedResponseOctet.GetExplicitBaseObject());
            byte[] responseBytes = responseOctet.GetOctets();

            Asn1Sequence responseContainer = Asn1Sequence.GetInstance(responseBytes);
            Asn1Sequence controlSequence = Asn1Sequence.GetInstance(responseContainer.ElementAt(0));
            Asn1Sequence statusInfoContainer = Asn1Sequence.GetInstance(controlSequence.ElementAt(0));

            Asn1Set statusInfoV2Container = Asn1Set.GetInstance(statusInfoContainer.ElementAt(2));
            Asn1Sequence statusInfoV2 = Asn1Sequence.GetInstance(statusInfoV2Container.ElementAt(0));

            //Read message status
            DerInteger encodedStatus = DerInteger.GetInstance(statusInfoV2.ElementAt(0));
            int status = encodedStatus.IntValueExact;

            //Read message content
            DerUtf8String encodedStatusString = DerUtf8String.GetInstance(statusInfoV2.ElementAt(2));
            string statusString = encodedStatusString.GetString();

            return new Tuple<int, string>(status, statusString);
        }

        public static X509Certificate2 ReadCmcCertificate(byte[] p7Bytes) {
            
            //Read wrapper
            Asn1Sequence p7 = Asn1Sequence.GetInstance(p7Bytes);
            Asn1TaggedObject taggedContainer = Asn1TaggedObject.GetInstance(p7.ElementAt(1));
            Asn1Sequence container = Asn1Sequence.GetInstance(taggedContainer.GetExplicitBaseObject());

            Asn1Sequence pkiResponse = Asn1Sequence.GetInstance(container.ElementAt(2));
            DerObjectIdentifier contentType = DerObjectIdentifier.GetInstance(pkiResponse.ElementAt(0));
            
            //Return if this is not a cert
            if (contentType.Id != CMC_DATA_OID) {
                return null;
            }

            //Read cert content
            Asn1TaggedObject taggedCert = Asn1TaggedObject.GetInstance(container.ElementAt(3));
            byte[] certBytes = taggedCert.GetExplicitBaseObject().GetEncoded();
            X509Certificate2 cert = new X509Certificate2(certBytes);

            return cert;
        }
    }
}