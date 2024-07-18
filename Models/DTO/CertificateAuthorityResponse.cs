
using System.Text.Json.Serialization;

namespace JellyfishTool.Models.DTO {

    public class CertificateAuthorityResponse {

        [JsonPropertyName("cas")]
        public CertificateAuthority[] CertificateAuthorities { get; set; }

        [JsonPropertyName("templates")]
        public CertificateTemplate[] CertificateTemplates { get; set; }
    }

    public class CertificateAuthority {

        [JsonPropertyName("id")]
        public string CaId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("friendlyName")]
        public string FriendlyName { get; set; }

        [JsonPropertyName("licensedTemplates")]
        public LicensedCertificateTemplate[] LicensedTemplates { get; set; }

        [JsonPropertyName("asymmetricAlgorithm")]
        public string AsymmetricAlgorithm { get; set; }

        [JsonPropertyName("function")]
        public string[] Functions { get; set; }

        [JsonPropertyName("vendor")]
        public string Vendor { get; set; }
    }

    public class LicensedCertificateTemplate {

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("licensedTemplateId")]
        public int LicensedTemplateId { get; set; }

        [JsonPropertyName("templateId")]
        public int TemplateId { get; set; }
    }

    public class CertificateTemplate {

        [JsonPropertyName("id")]
        public int TemplateId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("oid")]
        public string ObjectIdentifier { get; set; }

        [JsonPropertyName("class")]
        public string Class { get; set; }
    }
}