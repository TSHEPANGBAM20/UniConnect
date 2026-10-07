namespace UniConnect.MAUI.Models
{
    // Represents a user returned by the backend.
    public class UserModel
    {
        // Matches the backend Student.id field.
        // The Java backend uses Long, so we use long here.
        public long Id { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string Email { get; set; }
    }

    // Data sent to the backend when logging in.
    public class LoginRequest
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }

    // The backend currently returns a Student directly after login.
    // It does NOT return a JWT token or a separate UserId.
    public class LoginResponse
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string Email { get; set; }
    }

    // Data sent to the backend when registering.
    public class RegisterRequest
    {
        public string Name { get; set; }

        public string Surname { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
    }
}
