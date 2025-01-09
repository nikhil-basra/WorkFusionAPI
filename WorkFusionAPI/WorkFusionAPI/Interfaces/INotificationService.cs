using WorkFusionAPI.Models;

namespace WorkFusionAPI.Interfaces
{
    public interface INotificationService
    {
        Task<int> AddNotification(NotificationModel notification);
        Task<List<NotificationModel>> GetNotificationsByUserId(int userId);
        Task MarkAsRead(int notificationId);

        Task<List<NotificationModel>> GetNotificationsByEntityAndRole(int entityId, int roleId);

        Task DeleteNotification(int notificationId);

        Task<int> CountUnreadNotificationsByEntityAndRole(int entityId, int roleId);

    }

}
