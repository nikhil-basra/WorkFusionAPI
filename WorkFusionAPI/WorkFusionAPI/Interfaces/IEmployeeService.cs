using WorkFusionAPI.Models;

namespace WorkFusionAPI.Interfaces
{
    public interface IEmployeeService
    {
        Task<EmployeeModel> GetEmployeeByUserIdAsync(int userId);
  
        Task<IEnumerable<EmployeeModel>> GetAllEmployeesAsync();
        Task<EmployeeModel> GetEmployeeByIdAsync(int employeeId);
        Task<bool> CreateEmployeeAsync(EmployeeModel newEmployee);

        Task<bool> UpdateEmployeeAsync(EmployeeModel employee);


        Task<List<EmployeeDto>> GetEmployeesByManagerIdAsync(int managerId);
    }
}
