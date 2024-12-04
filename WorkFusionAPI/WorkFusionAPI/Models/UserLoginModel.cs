namespace WorkFusionAPI.Models
{
    public class UserLoginModel
    {
        public int RoleId { get; set; }
        public string UsernameOrEmail { get; set; }
        public string Password { get; set; } // Password in plain text, will hash it in the service
    }
}
