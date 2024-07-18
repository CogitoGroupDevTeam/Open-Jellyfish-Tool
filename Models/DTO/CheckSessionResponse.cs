
using System.Text.Json.Serialization;

namespace JellyfishTool.Models.DTO {

    public class CheckSessionResponse {
        
        [JsonPropertyName("tenancy-id")]
        public string[] TenantId { get; set; } 
        
        [JsonPropertyName("user-id")]
        public string[] UserId { get; set; } 
    }
}