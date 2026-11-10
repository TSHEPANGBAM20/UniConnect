
using System.Text.Json.Serialization;

namespace UniConnect.MAUI.Models
{
    // Stores details of a chat message received from the backend.
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

        public bool IsOwnMessage { get; set; }
    }

    // Request sent when a user sends a chat message.
    public class SendMessageRequest
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
