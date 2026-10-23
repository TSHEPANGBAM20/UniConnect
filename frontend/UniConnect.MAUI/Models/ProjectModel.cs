using System.Text.Json.Serialization;
namespace UniConnect.MAUI.Models

{
    public class ProjectModel
    {
        [JsonPropertyName("id")]
        public long ProjectId { get; set; }

        [JsonPropertyName("projectName")]
        public string ProjectName { get; set; }

        [JsonPropertyName("projectDescription")]
        public string ProjectDescription { get; set; }

        [JsonPropertyName("startDate")]
        public DateOnly StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateOnly EndDate { get; set; }

        [JsonPropertyName("projectFile")]
        public string ProjectFileUrl { get; set; }

        [JsonPropertyName("status")]
        public string ProjectStatus { get; set; }
        public List<MemberModel> Members { get; set; } = new();
    }

    public class MemberModel
    {
        public long UserId { get; set; }
        public string Name { get; set; }

        public string Email { get; set; }
        public string Role { get; set; }
    }

    public class CreateProjectRequest
    {
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }

    public class AddMemberRequest
    {
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
