using Dapper;
using WorkFusionAPI.Interfaces;
using WorkFusionAPI.Models;
using WorkFusionAPI.Utility;

namespace WorkFusionAPI.Services
{
    public class NotificationService : INotificationService
    {
        private readonly DBGateway _dbGateway;

        public NotificationService(DBGateway dbGateway)
        {
            _dbGateway = dbGateway;
        }

        public async Task<int> AddNotification(NotificationModel notification)
        {
            var query = @"INSERT INTO Notifications (EntityId,RoleId, Message, IsRead, CreatedAt)
                      VALUES (@EntityId, @RoleId, @Message, @IsRead, @CreatedAt)";
            var parameters = new DynamicParameters();
            parameters.Add("EntityId", notification.EntityId);
            parameters.Add("RoleId", notification.RoleId);
            parameters.Add("Message", notification.Message);
            parameters.Add("IsRead", notification.IsRead);
            parameters.Add("CreatedAt", notification.CreatedAt);
            return await _dbGateway.ExeQuery(query, parameters);
        }

        public async Task<List<NotificationModel>> GetNotificationsByUserId(int userId)
        {
            var query = @"SELECT * FROM Notifications WHERE UserId = @UserId ORDER BY CreatedAt DESC";
            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);
            return await _dbGateway.ExeQueryList<NotificationModel>(query, parameters);
        }

        public async Task MarkAsRead(int notificationId)
        {
            var query = @"UPDATE Notifications SET IsRead = 1 WHERE NotificationId = @NotificationId";
            var parameters = new DynamicParameters();
            parameters.Add("NotificationId", notificationId);
            await _dbGateway.ExeQuery(query, parameters);
        }

        public async Task<List<NotificationModel>> GetNotificationsByEntityAndRole(int entityId, int roleId)
        {
            var query = @"SELECT * FROM Notifications WHERE EntityId = @EntityId AND RoleId = @RoleId ORDER BY CreatedAt DESC";
            var parameters = new DynamicParameters();
            parameters.Add("EntityId", entityId);
            parameters.Add("RoleId", roleId);
            return await _dbGateway.ExeQueryList<NotificationModel>(query, parameters);
        }

        public async Task DeleteNotification(int notificationId)
        {
            var query = @"DELETE FROM Notifications WHERE NotificationId = @NotificationId";
            var parameters = new DynamicParameters();
            parameters.Add("NotificationId", notificationId);
            await _dbGateway.ExeQuery(query, parameters);
        }


        public async Task<int> CountUnreadNotificationsByEntityAndRole(int entityId, int roleId)
        {
            var query = @"SELECT COUNT(*) FROM Notifications 
                          WHERE EntityId = @EntityId AND RoleId = @RoleId AND IsRead = 0";

            var parameters = new DynamicParameters();
            parameters.Add("EntityId", entityId);
            parameters.Add("RoleId", roleId);

            return await _dbGateway.ExeScalarQuery<int>(query, parameters);
        }

    }
}
