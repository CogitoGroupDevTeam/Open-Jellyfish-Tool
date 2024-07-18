
namespace JellyfishTool.Models {

    public class CertificateTemplate {
        
        public int TemplateId { get; set;}

        public int LicensedTemplateId { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public bool Enabled { get; set; }
    }
}