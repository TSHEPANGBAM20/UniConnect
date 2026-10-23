using System.Text.Json.Serialization;
namespace UniConnect.MAUI.Models
{
    // This class stores the details of a chat message received from the backend.
    // These properties are used to display the message and identify who sent it.
    public class MessageModel
    {
        [JsonPropertyName("id")]
        public long MessageId { get; set; }

        [JsonPropertyName("chatId")]
        public long ChatId { get; set; }
        [JsonPropertyName("senderId")]
        public long SenderId { get; set; }

        [JsonPropertyName("sender")]
        public UserModel Sender { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("timeSent")]
        public DateTime TimeSent { get; set; }

        // This checks whether the message belongs to the logged-in user so the chat
        // screen can display their messages differently from messages sent by others.
        public bool IsOwnMessage { get; set; }
    }

    // This holds the message text that the frontend sends to the backend when
    // the user sends a new chat message.
    public class SendMessageRequest
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    // This stores notification details for displaying notifications in the app.
    public class NotificationModel
    {
        public int NotificationId { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
    }
}
