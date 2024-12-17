using WorkFusionAPI.Models;

namespace WorkFusionAPI.Interfaces
{
    public interface IClientsProjectRequestsService
    {
        Task<IEnumerable<ClientsProjectRequestsModel>> GetAllProjectsRequestsAsync();
        Task<ClientsProjectRequestsModel> GetProjectRequestsByIdAsync(int projectrequestId);
        Task<bool> AddProjectRequestAsync(ClientsProjectRequestsModel projectRequest);
        Task<bool> UpdateProjectRequestAsync(ClientsProjectRequestsModel projectRequest);
        Task<bool> DeleteProjectRequestAsync(int projectrequestId);

        Task<IEnumerable<ClientsProjectRequestsModel>> GetProjectRequestsByManagerAsync(int managerId);
    }
}
