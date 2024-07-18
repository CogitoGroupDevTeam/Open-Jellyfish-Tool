
using System.Text.Json.Serialization;

namespace JellyfishTool.Models.DTO {

    public class RequestCmcRequest {

        [JsonPropertyName("ca_id")]
        public int CaId { get; set; }

        [JsonPropertyName("licensed_template_id")]
        public int LicensedTemplateId { get; set; }

        [JsonPropertyName("cmc_request")]
        public string Cmc { get; set; }
    }
}