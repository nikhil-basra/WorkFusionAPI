using System.Collections.Generic;
using System.Threading.Tasks;
using WorkFusionAPI.Models;

namespace WorkFusionAPI.Interfaces
{
    public interface IClientService
    {
        Task<IEnumerable<ClientModel>> GetAllClientsAsync();
        Task<ClientModel> GetClientByIdAsync(int clientId);
        Task<bool> CreateClientAsync(ClientModel newClient);
        Task<bool> UpdateClientAsync(ClientModel client);
        Task<bool> DeleteClientAsync(int clientId);
    }
}
