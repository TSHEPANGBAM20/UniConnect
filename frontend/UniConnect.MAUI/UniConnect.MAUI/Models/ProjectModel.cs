namespace UniConnect.MAUI.Models
{
    public class ProjectModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ProjectFileUrl { get; set; }
        public string Role { get; set; }
        public List<MemberModel> Members { get; set; } = new();
    }

    public class MemberModel
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }

    public class CreateProjectRequest
    {
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class AddMemberRequest
    {
        public string Email { get; set; }
        public string Role { get; set; } = "TEAM_MEMBER";
    }
}
