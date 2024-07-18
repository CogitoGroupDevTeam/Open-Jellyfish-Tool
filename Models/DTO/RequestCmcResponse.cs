
using System.Text.Json.Serialization;

namespace JellyfishTool.Models.DTO {

    public class RequestCmcResponse {

        [JsonPropertyName("cmcResponse")]
        public string Cmc { get; set; }
    }
}