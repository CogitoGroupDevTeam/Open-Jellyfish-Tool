
using System.Text.Json.Serialization;

namespace JellyfishTool.Models.DTO {

    public class JellyfishVersionsResponse {
        
        [JsonPropertyName("product")]
        public string Product { get; set; } 
    }
}