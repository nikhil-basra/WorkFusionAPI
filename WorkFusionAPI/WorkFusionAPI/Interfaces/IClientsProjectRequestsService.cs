using WorkFusionAPI.Models;

namespace WorkFusionAPI.Interfaces
{
    public interface IClientsProjectRequestsService
    {
        Task<IEnumerable<ClientsProjectRequestsModel>> GetAllProjectsAsync();
        Task<ClientsProjectRequestsModel> GetProjectByIdAsync(int projectId);
        Task<bool> AddProjectRequestAsync(ClientsProjectRequestsModel projectRequest);
        Task<bool> UpdateProjectRequestAsync(ClientsProjectRequestsModel projectRequest);
        Task<bool> DeleteProjectRequestAsync(int projectId);
    }
}
