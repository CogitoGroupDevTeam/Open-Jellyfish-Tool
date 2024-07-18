
using System.Linq;

using JellyfishTool.Models.DTO;

namespace JellyfishTool.Models {

    public class CertificateAuthority {

        public int CaId { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public CertificateTemplate[] CertificateTemplates { get; set; }

        public static CertificateAuthority[] ParseDTO(CertificateAuthorityResponse dto) {

            CertificateAuthority[] cas = dto.CertificateAuthorities
                .Select(ca => {
                    
                    CertificateTemplate[] templates = ca.LicensedTemplates
                        .Select(license => {

                        DTO.CertificateTemplate template = dto.CertificateTemplates
                            .FirstOrDefault(template => template.TemplateId == license.TemplateId);

                        return new CertificateTemplate() {
                            TemplateId = template.TemplateId,
                            LicensedTemplateId = license.LicensedTemplateId,
                            Name = template.Name,
                            DisplayName = template.DisplayName,
                            Enabled = license.Enabled
                        };
                    })
                    .ToArray();

                    return new CertificateAuthority() {
                        CaId = int.Parse(ca.CaId),
                        Name = ca.Name,
                        DisplayName = ca.FriendlyName,
                        CertificateTemplates = templates
                    };
                })
                .ToArray();
            
            return cas;
        }
    }
}