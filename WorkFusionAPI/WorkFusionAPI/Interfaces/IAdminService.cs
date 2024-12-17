using WorkFusionAPI.Models;

namespace WorkFusionAPI.Interfaces
{
    public interface IAdminService
    {
        Task<AdminModel> GetAdminByUserIdAsync(int userId);
        Task<bool> UpdateAdminByUserIdAsync(int userId, AdminModel admin);
    
    }
}
