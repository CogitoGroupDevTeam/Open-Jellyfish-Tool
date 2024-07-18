
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using JellyfishTool.Models;
using JellyfishTool.Services;

namespace JellyfishTool.Modules.Features {

    public class CertificateRevocationModule : IFeatureModule {

        private readonly LicenseService license;
        private readonly RevocationService revocation;

        public string Name => "Submit Certificate Revocation Request"; 

        public CertificateRevocationModule(
            LicenseService license,
            RevocationService revocation
        ) {
            this.license = license;
            this.revocation = revocation;
        }

        public async Task Invoke()
        {

            //Prompt serial
            string serial = PromptSerial();

            //Prompt issuer
            Tuple<int, string> issuer = await PromptLicensedCertificateAuthoritySubject(license);

            //Prompt revocation reason
            RevocationReason reason = PromptRevocationReason();

            //Submit request
            try {
                await revocation.RevokeCertificate(
                    issuer.Item2,
                    serial,
                    reason
                );
            } catch (Exception ex) {
                Console.WriteLine($"Error, certificate not evoked: {ex.Message}");
                Console.WriteLine();
                return;
            }

            Console.WriteLine("Certificate successfully revoked");
            Console.WriteLine();
        }

        public static string PromptSerial() {

            Console.WriteLine("Enter revocation certificate serial as hex string:");
            string input = Console.ReadLine().Trim();

            return input;
        }

        public static async Task<Tuple<int, string>> PromptLicensedCertificateAuthoritySubject(LicenseService license) {

            CertificateAuthority[] authorities = await license.GetAllCertificateAuthorities();

            //Confirm
            Func<CertificateAuthority, string> getName = authority => authority.DisplayName;
            CertificateAuthority licensedAuthority = MainModule.PromptOption<CertificateAuthority>(authorities, getName);
            Console.WriteLine($"Using Certificate Authority: {licensedAuthority.DisplayName} ({licensedAuthority.CaId})");
            Console.WriteLine();

            return new Tuple<int, string>(licensedAuthority.CaId, licensedAuthority.Name);
        }

        public static RevocationReason PromptRevocationReason() {

            IEnumerable<RevocationReason> reasons = (IEnumerable<RevocationReason>)Enum.GetValues(typeof(RevocationReason));
            
            Func<RevocationReason, string> getName = reason => {
                string description = Enum.GetName(typeof(RevocationReason), reason);
                return $"{description}({(int)reason})";
            };
            RevocationReason reason = MainModule.PromptOption<RevocationReason>(reasons, getName);
            
            Console.WriteLine($"Using Revocation Reason: {(int)reason}");
            Console.WriteLine();

            return reason;
        }

        public static string PromptComment() {

            Console.WriteLine("Enter comment (optional):");
            string input = Console.ReadLine().Trim();

            return input;
        }
    }
}