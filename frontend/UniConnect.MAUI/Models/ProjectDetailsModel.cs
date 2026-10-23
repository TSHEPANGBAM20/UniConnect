using System.Text.Json.Serialization;

namespace UniConnect.MAUI.Models
{
    // Role names used by the backend (ProjectRoles enum: LEADER, MEMBER).
    // "TEAM_LEAD" is accepted too because the older sample data used it.
    public static class RoleNames
    {
        public static bool IsLeader(string? role) => role is "LEADER" or "TEAM_LEAD";
        public static string Display(string? role) => IsLeader(role) ? "Team Lead" : "Team Member";
    }

    // Everything the Project Details screen shows in section 1.
    // BACKEND: returned by GET /projects/{projectId}/details (see docs/API_PROJECT_DETAILS.md)
    public class ProjectDetailsModel
    {
        [JsonPropertyName("id")]
        public long ProjectId { get; set; }

        [JsonPropertyName("projectName")]       
        public string ProjectName { get; set; }

        [JsonPropertyName("projectDescription")]
        public string? ProjectDescription { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime? StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; } 
        
        [JsonPropertyName("projectFile")]         
        public string? ProjectFileName { get; set; } // null when no file was uploaded

        [JsonPropertyName("projectCode")]
        public int? ProjectCode { get; set; }        // 6-digit join code
        public string? MyRole { get; set; }          // role of the logged-in student: LEADER | MEMBER
    }

    // One row in section 2 (Members).
    // BACKEND: returned by GET /projects/{projectId}/members
    public class ProjectMemberModel
    {
        [JsonPropertyName("id")]
        public long MemberId { get; set; }
        
         [JsonPropertyName("project")]
        public ProjectModel Project { get; set; }
        public UserModel Student { get; set; }
        public string? Role { get; set; }            // LEADER | MEMBER

        // UI helpers (not part of the JSON)
        [JsonIgnore] public string FullName => $"{Student.Name} {Student.Surname}".Trim();
        [JsonIgnore] public bool IsLeader => RoleNames.IsLeader(Role);
        [JsonIgnore] public string RoleText => RoleNames.Display(Role);
        // Set by the ViewModel: true only when the viewer is the Team Lead and this row is not the lead.
        [JsonIgnore] public bool CanRemove { get; set; }
    }
}

namespace UniConnect.MAUI.Models
{
    // One uploaded project file or picture (section 2 of the Project Details screen).
    // BACKEND: returned by GET /projects/{projectId}/files and POST /projects/{projectId}/files
    public class ProjectFileModel
    {
        public long FileId { get; set; }
        public string? FileName { get; set; }       // original name, e.g. "Wireframes.png"
        public string? ContentType { get; set; }    // e.g. "image/png", "application/pdf"
        public long SizeBytes { get; set; }
        public long UploadedById { get; set; }       // student id of the uploader
        public string? UploadedByName { get; set; } // "Name Surname" of the uploader
        public DateTime? UploadedAt { get; set; }

        // UI helpers (not part of the JSON)
        [System.Text.Json.Serialization.JsonIgnore]
        public string SizeText => SizeBytes switch
        {
            < 1024 => $"{SizeBytes} B",
            < 1024 * 1024 => $"{SizeBytes / 1024.0:0} KB",
            _ => $"{SizeBytes / (1024.0 * 1024.0):0.0} MB"
        };

        [System.Text.Json.Serialization.JsonIgnore]
        public string InfoText =>
            $"{SizeText} - {UploadedByName ?? "Unknown"} - {UploadedAt?.ToString("d MMM yyyy", System.Globalization.CultureInfo.InvariantCulture) ?? "-"}";

        // Set by the ViewModel: true for the Team Lead and for the person who uploaded the file.
        [System.Text.Json.Serialization.JsonIgnore]
        public bool CanDelete { get; set; }
    }
}
