
using System.Text.Json.Serialization;

namespace JellyfishTool.Models.DTO {

    public class RequestRevocationRequest {

        [JsonPropertyName("caname")]
        public string CaName { get; set; }

        [JsonPropertyName("serial")]
        public string Serial { get; set; }

        [JsonPropertyName("reason")]
        public int Reason { get; set; }
    }
}