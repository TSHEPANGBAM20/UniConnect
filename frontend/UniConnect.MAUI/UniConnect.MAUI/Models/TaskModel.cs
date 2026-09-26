namespace UniConnect.MAUI.Models
{
    public class TaskModel
    {
        public int TaskId { get; set; }
        public int ProjectId { get; set; }
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
        public int? AssignedTo { get; set; }
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
