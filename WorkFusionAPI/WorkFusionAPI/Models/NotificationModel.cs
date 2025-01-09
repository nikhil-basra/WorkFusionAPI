namespace WorkFusionAPI.Models
{
    public class NotificationModel
    {
        public int NotificationId { get; set; }
        public int EntityId { get; set; }
        public int RoleId { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
