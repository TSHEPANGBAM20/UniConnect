
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

        // FRONTEND:
        // Check whether the task has been completed.
        [JsonIgnore]
        public bool IsCompleted =>
            string.Equals(Status?.Trim(), "DONE",
                StringComparison.OrdinalIgnoreCase)
            ||
            string.Equals(Status?.Trim(), "COMPLETED",
                StringComparison.OrdinalIgnoreCase);

        // FRONTEND:
        // A task is overdue when its deadline has passed
        // and it has not been completed.
        [JsonIgnore]
        public bool IsOverdue =>
            !IsCompleted &&
            DueDate.HasValue &&
            DueDate.Value.Date < DateTime.Today;

        // FRONTEND:
        // Display OVERDUE without changing the backend status.
        [JsonIgnore]
        public string DisplayStatus
        {
            get
            {
                if (IsOverdue)
                    return "OVERDUE";

                if (IsCompleted)
                    return "DONE";

                if (string.Equals(Status?.Trim(), "IN_PROGRESS",
                    StringComparison.OrdinalIgnoreCase))
                    return "IN PROGRESS";

                if (string.Equals(Status?.Trim(), "NOT_STARTED",
                    StringComparison.OrdinalIgnoreCase))
                    return "PENDING";

                return Status ?? "PENDING";
            }
        }
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
