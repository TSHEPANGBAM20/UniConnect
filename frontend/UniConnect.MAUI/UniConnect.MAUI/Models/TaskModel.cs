namespace UniConnect.MAUI.Models
{
    public class TaskModel
    {
        public int TaskId { get; set; }

        public int ProjectId { get; set; }

        public string TaskTitle { get; set; }

        public string TaskDescription { get; set; }

        // Priority levels: Low, Medium, High.
        public string Priority { get; set; }

        // Progress/status of the task.
        // Intended values:
        // NOT_STARTED = task has not started
        // IN_PROGRESS = task is currently being worked on
        // DONE = task is completed
        public string Status { get; set; }

        // Date the task is scheduled to start.
        public DateTime? StartDate { get; set; }

        // Person responsible for completing the task.
        public int? AssignedTo { get; set; }

        // Display name of the assigned team member.
        public string AssignedToName { get; set; }

        // Date the task is due.
        public DateTime? DueDate { get; set; }
    }

    public class CreateTaskRequest
    {
        public string TaskTitle { get; set; }

        public string TaskDescription { get; set; }

        public string Priority { get; set; }

        public int? AssignedTo { get; set; }

        // Date the task should start.
        public DateTime? StartDate { get; set; }

        // Date the task must be completed by.
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
