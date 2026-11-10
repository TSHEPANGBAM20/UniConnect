
using System.Text.Json.Serialization;

namespace UniConnect.MAUI.Models
{
    public class TaskModel
    {
        // BACKEND:
        // Return a unique ID for every task.
        public long TaskId { get; set; }

        // BACKEND:
        // Return the project associated with this task.
        [JsonPropertyName("project")]
        public ProjectModel Project { get; set; }

        // BACKEND:
        // The backend JSON response must use the property name "title".
        [JsonPropertyName("title")]
        public string TaskTitle { get; set; }

        // BACKEND:
        // Return the task description.
        public string TaskDescription { get; set; }

        // BACKEND:
        // Return the task priority, for example HIGH, MEDIUM or LOW.
        public string Priority { get; set; }

        // BACKEND:
        // Return the current task status.
        // Use DONE for completed tasks.
        public string Status { get; set; }

        // BACKEND:
        // Return the ID of the assigned student, if applicable.
        public int? AssignedTo { get; set; }

        // BACKEND:
        // Return the name of the assigned student, if available.
        public string AssignedToName { get; set; }

        // BACKEND:
        // Return the task deadline.
        // The backend uses LocalDate for this field.
        public DateTime? DueDate { get; set; }
    }

    // BACKEND:
    // Request model used when creating a task.
    public class CreateTaskRequest
    {
        public string TaskTitle { get; set; }
        public string TaskDescription { get; set; }
        public string Priority { get; set; }
        public long? AssignedTo { get; set; }

        // BACKEND:
        // Send the deadline as YYYY-MM-DD.
        // Do not send a time or timezone.
        [JsonIgnore]
        public DateTime? DueDate { get; set; }

        [JsonPropertyName("DueDate")]
        public string? DueDateForBackend =>
            DueDate?.ToString("yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture);
    }

    // BACKEND:
    // Request model used when updating a task's status.
    public class UpdateTaskStatusRequest
    {
        public string Status { get; set; }
    }

    // BACKEND:
    // Model for calendar entries returned by the calendar feature.
    public class CalendarItem
    {
        public string Type { get; set; }
        public int RefId { get; set; }
        public string Title { get; set; }
        public DateTime DueDate { get; set; }
    }
}
