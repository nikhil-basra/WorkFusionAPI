using System.Collections.Generic;
using System.Threading.Tasks;
using WorkFusionAPI.Models;

namespace WorkFusionAPI.Interfaces
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectModel>> GetProjectsAsync();
        Task<ProjectModel> GetProjectByIdAsync(int projectId);
        Task<bool> AddProjectAsync(ProjectModel project);
        Task<bool> UpdateProjectAsync(ProjectModel project);
        Task<bool> DeleteProjectAsync(int projectId);
    }
}
