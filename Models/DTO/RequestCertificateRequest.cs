
using System.Text.Json.Serialization;

namespace JellyfishTool.Models.DTO {

    public class RequestCertificateRequest {

        [JsonPropertyName("ca_id")]
        public int CaId { get; set; }

        [JsonPropertyName("licensed_template_id")]
        public int LicensedTemplateId { get; set; }

        [JsonPropertyName("csr")]
        public string Csr { get; set; }
    }
}