namespace WorkFusionAPI.Models
{
    public class UserLoginModel
    {
        public int RoleId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // Password in plain text, will hash it in the service
    }
}
