using Dapper;
using System.Data;
using WorkFusionAPI.Interfaces;
using WorkFusionAPI.Models;
using WorkFusionAPI.Utility;

namespace WorkFusionAPI.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly DBGateway _dbGateway;

        public EmployeeService(DBGateway dbGateway)
        {
            _dbGateway = dbGateway;
        }

        public async Task<EmployeeModel> GetEmployeeByUserIdAsync(int userId)
        {
            string query = "SELECT * FROM Employees WHERE UserId = @UserId";
            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);
            return await _dbGateway.ExeScalarQuery<EmployeeModel>(query, parameters);
        }



        public async Task<IEnumerable<EmployeeModel>> GetAllEmployeesAsync()
        {
            var query = "SELECT * FROM Employees WHERE IsActive = true";
            return await _dbGateway.ExeQueryList<EmployeeModel>(query);
        }

        public async Task<EmployeeModel> GetEmployeeByIdAsync(int employeeId)
        {
            var query = "SELECT * FROM Employees WHERE EmployeeId = @EmployeeId AND IsActive = true";
            var parameters = new DynamicParameters();
            parameters.Add("@EmployeeId", employeeId);
            return await _dbGateway.ExeScalarQuery<EmployeeModel>(query, parameters);
        }




        public async Task<bool> CreateEmployeeAsync(EmployeeModel newEmployee)
        {

            // Set IsActive to true by default
            newEmployee.IsActive = newEmployee.IsActive || true;

            // Handle EmployeeImage if it's already a Base64 string
            if (!string.IsNullOrEmpty(newEmployee.EmployeeImage))
            {
                try
                {
                    // If EmployeeImage is already Base64, leave as is; otherwise, convert it
                    if (!newEmployee.EmployeeImage.StartsWith("data:image"))
                    {
                        newEmployee.EmployeeImage = Convert.ToBase64String(Convert.FromBase64String(newEmployee.EmployeeImage));
                    }
                }
                catch (FormatException)
                {
                    // If it’s not in Base64 format, attempt to read it as a file path
                    var imageBytes = await File.ReadAllBytesAsync(newEmployee.EmployeeImage);
                    newEmployee.EmployeeImage = Convert.ToBase64String(imageBytes);
                }
            }

            // Ensure CreatedAt and UpdatedAt are set to current DateTime
            newEmployee.CreatedAt = DateTime.Now; // Assigning DateTime value directly
            newEmployee.UpdatedAt = DateTime.Now; // Assigning DateTime value directly

            var insertQuery = @"
        INSERT INTO Employees (FirstName, LastName, Gender, Email, Phone, PresentAddress, PermanentAddress, IDType, IDNumber, 
                               DateOfBirth, DepartmentId, UserId, HireDate, CurrentSalary, EmployeeImage, 
                               IsActive, CreatedAt, UpdatedAt)
        VALUES (@FirstName, @LastName, @Gender, @Email, @Phone, @PresentAddress, @PermanentAddress, @IDType, @IDNumber, 
                @DateOfBirth, @DepartmentId, @UserId, @HireDate, @CurrentSalary, @EmployeeImage, 
                @IsActive, @CreatedAt, @UpdatedAt)";

            var parameters = new DynamicParameters(newEmployee);

            // Execute query to insert employee data
            var result = await _dbGateway.ExeQuery(insertQuery, parameters);

            // Return true if rows were affected (employee created), otherwise false
            return result > 0;
        }
        public async Task<bool> UpdateEmployeeAsync(EmployeeModel employee)
        {
            employee.UpdatedAt = DateTime.Now;

            // Process EmployeeImage for Base64 format if provided
            if (!string.IsNullOrEmpty(employee.EmployeeImage))
            {
                try
                {
                    if (!employee.EmployeeImage.StartsWith("data:image"))
                    {
                        employee.EmployeeImage = Convert.ToBase64String(Convert.FromBase64String(employee.EmployeeImage));
                    }
                }
                catch (FormatException)
                {
                    var imageBytes = await File.ReadAllBytesAsync(employee.EmployeeImage);
                    employee.EmployeeImage = Convert.ToBase64String(imageBytes);
                }
            }
            else
            {
                var existingEmployee = await GetEmployeeByIdAsync(employee.EmployeeId);
                if (existingEmployee != null)
                {
                    employee.EmployeeImage = existingEmployee.EmployeeImage;
                }
            }

            // SQL for updating Employee and User tables
            var query = @"

 -- Update User table FullName and Email based on the previous email
        UPDATE users 
        SET FullName = CONCAT(@FirstName, ' ', @LastName), 
            Email = @Email
        WHERE Email = (SELECT Email FROM Employees WHERE EmployeeId = @EmployeeId);

        UPDATE Employees SET 
            FirstName = @FirstName,
            LastName = @LastName,
            Gender = @Gender,
            Email = @Email,
            Phone = @Phone,
            PresentAddress = @PresentAddress,
            PermanentAddress = @PermanentAddress,
            IDType = @IDType,
            IDNumber = @IDNumber,
            DateOfBirth = @DateOfBirth,
            DepartmentId = @DepartmentId,
            HireDate = @HireDate,
            CurrentSalary = @CurrentSalary,
            EmployeeImage = @EmployeeImage,
            UpdatedAt = @UpdatedAt
        WHERE EmployeeId = @EmployeeId AND IsActive = true;";

            var parameters = new DynamicParameters(employee);
            var result = await _dbGateway.ExeQuery(query, parameters);

            return result > 0;
        }



        public async Task<List<EmployeeDto>> GetEmployeesByManagerIdAsync(int managerId)
        {
            string query = @"
                SELECT e.EmployeeId, e.FirstName, e.LastName, e.EmployeeImage 
                FROM employees e
                INNER JOIN managers m ON m.DepartmentId = e.DepartmentId
                WHERE m.ManagerId = @ManagerId;";

            var parameters = new DynamicParameters();
            parameters.Add("ManagerId", managerId);

            return await _dbGateway.ExeQueryList<EmployeeDto>(query, parameters);
        }
    }
}
