using System.Collections.Generic;
using System.Threading.Tasks;
using WorkFusionAPI.Models;

namespace WorkFusionAPI.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentModel>> GetDepartmentsAsync();
        Task<DepartmentModel> GetDepartmentByIdAsync(int departmentId);
        Task<bool> AddDepartmentAsync(DepartmentModel department);
        Task<bool> UpdateDepartmentAsync(DepartmentModel department);
        Task<List<DepartmentEmployeeCountModel>> GetActiveEmployeeCountsAsync();
    }
}
