
using System;
using System.Threading.Tasks;

using JellyfishTool.Models;
using JellyfishTool.Models.DTO;
using JellyfishTool.Services;

namespace JellyfishTool.Modules.Features {

    public class CmcCertificateRevocationModule : IFeatureModule {

        private readonly LicenseService license;
        private readonly CmcService cmc;

        private readonly CryptographicModule crypto;

        public string Name => "Submit Certificate Revocation Request in CMC Format";

        public CmcCertificateRevocationModule(
            LicenseService license,
            CmcService cmc,
            CryptographicModule crypto
        ) {
            this.license = license;
            this.cmc = cmc;
            this.crypto = crypto;
        }

        public async Task Invoke() {

            //Ensure there are signer keys
            bool keysExist = await crypto.AssertCrypto();
            if (!keysExist) {
                Console.WriteLine("Cryptographic keys are not specified, CMC not available");
                return;
            }

            //Prompt serial
            string serial = CertificateRevocationModule.PromptSerial();

            //Prompt issuer
            Tuple<int, string> issuer = await CertificateRevocationModule.PromptLicensedCertificateAuthoritySubject(license);

            //Prompt revocation reason
            RevocationReason reason = CertificateRevocationModule.PromptRevocationReason();

            //Prompt comment (optional)
            string comment = CertificateRevocationModule.PromptComment();

            //Create revocation request
            string requestPem;
            try {
                requestPem = cmc.CreateCmcRevocation(
                    issuer.Item2,
                    serial,
                    reason,
                    comment
                );
            } catch (Exception ex) {
                Console.WriteLine();
                Console.WriteLine($"Cannot convert create CMC revocation request: {ex.Message}");
                return;
            }

            //Prompt to save CMC request
            Console.WriteLine();
            await CmcCertificateRequestModule.PromptSaveCmc(requestPem);

            //Submit CMC request
            RequestCmcResponse cmcResponse;
            try {
                cmcResponse = await cmc.RequestRevoke(
                    issuer.Item1,   //CA Id
                    requestPem      //Cmc Data
                );
            } catch (Exception ex) {
                Console.WriteLine();
                Console.WriteLine($"Error, revocation not complete: {ex.Message}");
                return;
            }
            string responsePem = cmcResponse.Cmc;

            //Parse CMC response
            byte[] p7Bytes = CryptoService.PemDecode(responsePem);
            
            //Read status message
            Tuple<int, string> status = CmcService.ReadCmcStatus(p7Bytes);
            if (status != null) {

                string statusMessage = 
                    status.Item1 == CmcService.CMC_STATUS_INFO_SUCCESS ? 
                    "Success" :
                    "Error, certificate not revoked";

                Console.WriteLine();
                Console.WriteLine($"{statusMessage}: {status.Item2}");
            }

            //Prompt to save CMC response
            Console.WriteLine();
            await CmcCertificateRequestModule.PromptSaveCmc(responsePem);
        }
    }
}