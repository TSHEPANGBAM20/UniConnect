namespace UniConnect.MAUI.Models
{
    public class MessageModel
    {
        public int MessageId { get; set; }
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsOwnMessage { get; set; }
    }

    public class SendMessageRequest
    {
        public string Message { get; set; }
    }

    public class NotificationModel
    {
        public int NotificationId { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
    }
}
