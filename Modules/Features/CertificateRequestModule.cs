
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using JellyfishTool.Models;
using DTO = JellyfishTool.Models.DTO;
using JellyfishTool.Services;

namespace JellyfishTool.Modules.Features {

    public class CertificateRequestModule : IFeatureModule {

        private readonly LicenseService license;
        private readonly CertificateService certificates;

        public string Name => "Submit Certificate Signing Request"; 

        public CertificateRequestModule(
            LicenseService license,
            CertificateService certificates
        ) {
            this.license = license;
            this.certificates = certificates;
        }

        public async Task Invoke()
        {
            //Get CA Id and Template Id
            Tuple<int, int> licensedTemplate = await PromptLicensedTemplate(license);

            //Get CSR
            string csr = await PromptCsr();
            if (string.IsNullOrEmpty(csr)) {
                Console.WriteLine("Invalid CSR, certificate not issued");
                return;
            }

            //Submit certificate request
            DTO.RequestCertificateResponse cert;
            try {
                cert = await certificates.RequestCertificate(
                    licensedTemplate.Item1,
                    licensedTemplate.Item2,
                    csr
                );
            } catch (Exception ex) {
                Console.WriteLine($"Error, certificate not issued: {ex.Message}");
                Console.WriteLine();
                return;
            }

            Console.WriteLine($"Issued certificate with serial: {cert.SerialNumber}");
            Console.WriteLine();

            await PromptSaveCert(cert.Cert);
        }

        public static async Task<Tuple<int, int>> PromptLicensedTemplate(LicenseService license) {

            CertificateAuthority[] authorities = await license.GetAllCertificateAuthorities();

            //Collate all options to simplify prompt numbering
            List<Tuple<CertificateAuthority, CertificateTemplate>> options = [];
            foreach(CertificateAuthority authority in authorities) {

                foreach(CertificateTemplate template in authority.CertificateTemplates) {

                    if (!template.Enabled) continue;
                    options.Add(new Tuple<CertificateAuthority, CertificateTemplate>(authority, template));
                }
            }      

            //Confirm
            Func<Tuple<CertificateAuthority, CertificateTemplate>, string> getName = option => $"{option.Item1.DisplayName}: {option.Item2.DisplayName}"; 
            Tuple<CertificateAuthority, CertificateTemplate> licensedTemplate = MainModule.PromptOption<Tuple<CertificateAuthority, CertificateTemplate>>(options, getName);
            Console.WriteLine($"Using Template: {licensedTemplate.Item1.DisplayName} ({licensedTemplate.Item1.CaId}): {licensedTemplate.Item2.DisplayName} ({licensedTemplate.Item2.LicensedTemplateId})");
            Console.WriteLine();

            return new Tuple<int, int>(licensedTemplate.Item1.CaId, licensedTemplate.Item2.LicensedTemplateId);
        }

        public static async Task<string> PromptCsr() {
            
            Console.WriteLine("Enter CSR path:");
            string input = Console.ReadLine().Trim();

            Console.WriteLine("Processing input...");
            return await CryptoService.CollectCertificateSigningRequest(input);
        }

        public static async Task PromptSaveCert(string certPem) {

            bool written = false;
            while (!written) {

                Console.WriteLine("Enter path to save issued certificate:");
                string path = Console.ReadLine().Trim();
                Console.WriteLine();
                
                try {

                    await FileSystemService.WriteAsText(certPem, path);

                    Console.WriteLine("Certificate saved");
                    written = true;

                } catch (Exception ex) {
                    Console.WriteLine($"Failed to save certificate: {ex.Message}");
                    Console.WriteLine();
                }
            }
        }
    }
}