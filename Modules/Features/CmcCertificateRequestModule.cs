
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using JellyfishTool.Models.DTO;
using JellyfishTool.Services;

namespace JellyfishTool.Modules.Features {

    public class CmcCertificateRequestModule : IFeatureModule {

        private readonly LicenseService license;
        private readonly CmcService cmc;

        private readonly CryptographicModule crypto;

        public string Name => "Submit Certificate Signing Request in CMC Format"; 

        public CmcCertificateRequestModule(
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

            //Get CA Id and Template Id
            Tuple<int, int> licensedTemplate = await CertificateRequestModule.PromptLicensedTemplate(license);

            //Get CSR
            string csr = await CertificateRequestModule.PromptCsr();
            if (string.IsNullOrEmpty(csr)) {
                Console.WriteLine();
                Console.WriteLine("Invalid CSR, certificate not issued");
                return;
            }

            //Wrap P10 in CMC format
            string requestPem;
            try {
                requestPem = cmc.ConvertPkcs10ToCmc(csr);
            } catch (Exception ex) {
                Console.WriteLine();
                Console.WriteLine($"Cannot convert CSR to CMC: {ex.Message}");
                return;
            }

            //Prompt to save CMC request
            Console.WriteLine();
            await PromptSaveCmc(requestPem);

            //Submit CMC request
            RequestCmcResponse cmcResponse;
            try {
                cmcResponse = await cmc.RequestCertificate(
                    licensedTemplate.Item1,     //CA Id
                    licensedTemplate.Item2,     //Licensed template Id
                    requestPem                  //Cmc Data
                );
            } catch (Exception ex) {
                Console.WriteLine();
                Console.WriteLine($"Error, certificate not issued: {ex.Message}");
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
                    "Error, certificate not issued";

                Console.WriteLine();
                Console.WriteLine($"{statusMessage}: {status.Item2}");
            }

            //Prompt to save CMC response
            Console.WriteLine();
            await PromptSaveCmc(responsePem);

            //Prompt to save cert
            X509Certificate2 cert = CmcService.ReadCmcCertificate(p7Bytes);
            if (cert != null) {
                string certPem = cert.ExportCertificatePem();
                await CertificateRequestModule.PromptSaveCert(certPem);
            }
        }

        public static async Task PromptSaveCmc(string cmcPem) {

            Console.WriteLine("Enter path to save CMC PEM (empty to skip):");
            string path = Console.ReadLine().Trim();
            Console.WriteLine();

            if (string.IsNullOrEmpty(path)) return;

            bool written = false;
            while (!written) {
                try {

                    await FileSystemService.WriteAsText(cmcPem, path);

                    Console.WriteLine("CMC PEM saved");
                    written = true;

                } catch (Exception ex) {
                    Console.WriteLine();
                    Console.WriteLine($"Failed to save CMC PEM: {ex.Message}");
                }
            }
        }
    }
}