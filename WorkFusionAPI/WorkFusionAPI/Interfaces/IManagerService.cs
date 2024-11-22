using System.Collections.Generic;
using System.Threading.Tasks;
using WorkFusionAPI.Models;

namespace WorkFusionAPI.Interfaces
{
    public interface IManagerService
    {
        Task<IEnumerable<ManagerModel>> GetAllManagersAsync();
        Task<ManagerModel> GetManagerByIdAsync(int managerId);
        Task<bool> CreateManagerAsync(ManagerModel newManager);
        Task<bool> UpdateManagerAsync(ManagerModel manager);
        Task<bool> DeleteManagerAsync(int managerId);
    }
}
