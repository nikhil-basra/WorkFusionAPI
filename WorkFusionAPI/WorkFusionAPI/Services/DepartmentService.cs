using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using WorkFusionAPI.Interfaces;
using WorkFusionAPI.Models;
using WorkFusionAPI.Utility;

namespace WorkFusionAPI.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly DBGateway _dbGateway;

        public DepartmentService(DBGateway dbGateway)
        {
            _dbGateway = dbGateway;
        }

        public async Task<IEnumerable<DepartmentModel>> GetDepartmentsAsync()
        {
            var query = "SELECT * FROM departments";
            return await _dbGateway.ExeQueryList<DepartmentModel>(query);
        }

        public async Task<DepartmentModel> GetDepartmentByIdAsync(int departmentId)
        {
            var query = "SELECT * FROM departments WHERE DepartmentId = @DepartmentId";
            var parameters = new DynamicParameters();
            parameters.Add("@DepartmentId", departmentId);
            return await _dbGateway.ExeScalarQuery<DepartmentModel>(query, parameters);
        }

        public async Task<bool> AddDepartmentAsync(DepartmentModel department)
        {
            var query = @"
                INSERT INTO departments (DepartmentName, Description, CreatedAt)
                VALUES (@DepartmentName, @Description, @CreatedAt)";
            var parameters = new DynamicParameters(department);
            return await _dbGateway.ExeQuery(query, parameters) > 0;
        }

        public async Task<bool> UpdateDepartmentAsync(DepartmentModel department)
        {
            var query = @"
        UPDATE departments
        SET DepartmentName = @DepartmentName,
            Description = @Description,
            UpdatedAt = CURRENT_TIMESTAMP
        WHERE DepartmentId = @DepartmentId";

            var parameters = new DynamicParameters(department);
            return await _dbGateway.ExeQuery(query, parameters) > 0;
        }

        public async Task<List<DepartmentEmployeeCountModel>> GetActiveEmployeeCountsAsync()
        {
            string query = @"
                SELECT 
                    d.DepartmentName, 
                    COUNT(e.EmployeeId) AS ActiveEmployeeCount
                FROM 
                    departments d
                LEFT JOIN 
                    employees e ON d.DepartmentId = e.DepartmentId AND e.IsActive = 1
                GROUP BY 
                    d.DepartmentId, d.DepartmentName;";

            return await _dbGateway.ExeQueryList<DepartmentEmployeeCountModel>(query);
        }

    }
}
