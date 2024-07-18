
using System.Text.Json.Serialization;

namespace JellyfishTool.Models.DTO {

    public class RequestCertificateResponse {

        [JsonPropertyName("requestId")]
        public string RequestId { get; set; }

        [JsonPropertyName("disposition")]
        public string Disposition { get; set; }

        [JsonPropertyName("cert")]
        public string Cert { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("serialnumber")]
        public string SerialNumber { get; set; }
    }
}