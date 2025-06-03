namespace UserAUTH.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public Guid PasswordGuid { get; set; }
        public Guid? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
    }

    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class ResetRequest
    {
        public string Email { get; set; }
        public Guid Token { get; set; }
        public string NewPassword { get; set; }
    }
}
