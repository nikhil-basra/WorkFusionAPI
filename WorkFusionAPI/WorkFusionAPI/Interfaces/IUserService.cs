using WorkFusionAPI.Models;

namespace WorkFusionAPI.Interfaces
{
    public interface IUserService
    {
        Task<int> RegisterUser(UserModel user);
        Task<string> EncryptPassword(string password);


        Task<IEnumerable<Users>> GetAllUsersAsync();
        Task<bool> UpdateUserIsActiveStatus(int userId, bool isActive);


        Task<IEnumerable<Users>> GetUsersByRoleIdAsync(int roleId);


        // New Methods for Password Reset
        Task<bool> VerifyUserCredentials(int userId, string username, string password);
        Task<bool> ResetPassword(int userId, string newPassword);

    }
}
