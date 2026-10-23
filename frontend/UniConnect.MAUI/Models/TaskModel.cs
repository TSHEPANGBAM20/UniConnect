using System.Text.Json.Serialization;
namespace UniConnect.MAUI.Models

{
    public class TaskModel
    {

        public long TaskId { get; set; }

        [JsonPropertyName("project")]
        public ProjectModel Project { get; set; }
        
        [JsonPropertyName("title")]
        public string TaskTitle { get; set; }
        public string TaskDescription { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public int? AssignedTo { get; set; }
        public string AssignedToName { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class CreateTaskRequest
    {

        public string TaskTitle { get; set; }
        public string TaskDescription { get; set; }
        public string Priority { get; set; }
        public long? AssignedTo { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class UpdateTaskStatusRequest
    {
        public string Status { get; set; }
    }

    public class CalendarItem
    {
        public string Type { get; set; }
        public int RefId { get; set; }
        public string Title { get; set; }
        public DateTime DueDate { get; set; }
    }
}
