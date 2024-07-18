
using System.Text.Json.Serialization;

namespace JellyfishTool.Models.DTO {

    public class ProjectsResponse {

        [JsonPropertyName("projects")]
        public Project[] Projects { get; set; }
    }

    public class Project {
        
        [JsonPropertyName("projectId")]
        public int ProjectId { get; set; }

        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}