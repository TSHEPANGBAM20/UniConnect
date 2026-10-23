using System.Text.Json.Serialization;

namespace UniConnect.MAUI.Models
{
    
    public static class RoleNames
    {
        public static bool IsLeader(string? role) => role is "LEADER" or "TEAM_LEAD";
        public static string Display(string? role) => IsLeader(role) ? "Team Lead" : "Team Member";
    }

    // Everything the Project Details screen shows in section 1.
   
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
        public string? MyRole { get; set; }          
    }

    
    public class ProjectMemberModel
    {
        [JsonPropertyName("id")]
        public long MemberId { get; set; }
        
         [JsonPropertyName("project")]
        public ProjectModel Project { get; set; }
        public UserModel Student { get; set; }
        public string? Role { get; set; }           

        
        [JsonIgnore] public string FullName => $"{Student.Name} {Student.Surname}".Trim();
        [JsonIgnore] public bool IsLeader => RoleNames.IsLeader(Role);
        [JsonIgnore] public string RoleText => RoleNames.Display(Role); // Set by the ViewModel: true only when the viewer is the Team Lead and this row is not the lead.
        [JsonIgnore] public bool CanRemove { get; set; }
    }
}

namespace UniConnect.MAUI.Models
{
    // returned by GET /projects/{projectId}/files and POST /projects/{projectId}/files
    public class ProjectFileModel
    {
        public long FileId { get; set; }
        public string? FileName { get; set; }       // original name
        public string? ContentType { get; set; }    // //type of file
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

        [System.Text.Json.Serialization.JsonIgnore]
        public bool CanDelete { get; set; }
    }
}
