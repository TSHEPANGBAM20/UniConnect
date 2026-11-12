
using System.Text.Json.Serialization;

namespace UniConnect.MAUI.Models
{
    public class NotificationModel
    {
        // Unique notification identifier.
        public long NotificationId { get; set; }

        // Task notification reference.
        public long TaskId { get; set; }

        // Project notification reference.
        public long ProjectId { get; set; }

        // Name of the project associated with the notification.
        public string ProjectName { get; set; } = string.Empty;

        // Main notification message.
        public string Message { get; set; } = string.Empty;

        // Notification heading.
        public string Title { get; set; } = string.Empty;

        // Deadline associated with the notification.
        public DateTime DueDate { get; set; }

        // Indicates whether the notification has been read.
        public bool IsRead { get; set; }

        // Notification type:
        // OVERDUE, DUE_TODAY, DUE_TOMORROW,
        // PROJECT_OVERDUE, PROJECT_DUE_TODAY,
        // PROJECT_DUE_TOMORROW, CHAT_MESSAGE,
        // NEW_MEMBER, PROJECT_UPDATE.
        public string NotificationType { get; set; } = string.Empty;

        // Identifies whether this notification relates to a task or project.
        public string RelatedType { get; set; } = string.Empty;

        // Optional sender name for chat notifications.
        public string SenderName { get; set; } = string.Empty;

        // Optional chat or message reference.
        public long MessageId { get; set; }

        // Optional time when the notification was created.
        public DateTime CreatedAt { get; set; }

        // FRONTEND: Display-friendly notification category.
        [JsonIgnore]
        public string DisplayTitle =>
            !string.IsNullOrWhiteSpace(Title)
                ? Title
                : NotificationType switch
                {
                    "OVERDUE" => "Task overdue",
                    "DUE_TODAY" => "Task due today",
                    "DUE_TOMORROW" => "Task due tomorrow",
                    "PROJECT_OVERDUE" => "Project overdue",
                    "PROJECT_DUE_TODAY" => "Project due today",
                    "PROJECT_DUE_TOMORROW" => "Project due tomorrow",
                    "CHAT_MESSAGE" => "New message",
                    "NEW_MEMBER" => "New team member",
                    "PROJECT_UPDATE" => "Project update",
                    _ => "Notification"
                };
    }
}
