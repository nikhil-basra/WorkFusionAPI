using System.Collections.Generic;
using System.Threading.Tasks;
using WorkFusionAPI.Models;

namespace WorkFusionAPI.Interfaces
{
    public interface IProjectsService
    {
        Task<IEnumerable<ProjectsModel>> GetAllProjectsAsync();
        Task<ProjectsModel> GetProjectByIdAsync(int projectId);
        Task<int> CreateProjectAsync(ProjectsModel project);
        Task<int> UpdateProjectAsync(ProjectsModel project);
        Task<int> DeleteProjectAsync(int projectId);
        Task<IEnumerable<ProjectsModel>> GetProjectsByManagerIdAsync(int managerId);
        Task<IEnumerable<ProjectsModel>> GetProjectsByClientIdAsync(int clientId);
        Task<IEnumerable<ProjectsModel>> GetProjectsByEmployeeIdAsync(int employeeId);


        //----------------counts for graphs--------------------------
        Task<ProjectStatusCountsModel> GetProjectStatusCountsByManagerIdAsync(int managerId);
       
        Task<ProjectStatusCountsModel> GetProjectStatusCountsAsync();


        Task<List<DepartmentProjectStatusCountsModel>> GetDepartmentProjectStatusCountsAsync();




    }

}