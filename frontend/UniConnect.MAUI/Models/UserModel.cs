using System.Text.Json.Serialization;
namespace UniConnect.MAUI.Models

{
    public class UserModel
    {
        //this is used in the dasboard i think
        [JsonPropertyName("id")]
        public long UserId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // Matches the Student object returned by the current Spring Boot backend.
    public class LoginResponse
    {
        [JsonPropertyName("id")]
        public long UserId { get; set; }

        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

// ended off with desirializing
    public class RegisterRequest
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}