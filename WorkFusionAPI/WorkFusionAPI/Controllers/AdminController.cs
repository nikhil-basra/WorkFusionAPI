using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkFusionAPI.Interfaces;
using WorkFusionAPI.Models;
using WorkFusionAPI.Services;

namespace WorkFusionAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "1")] // Restrict access to Admin role

    public class AdminController : ControllerBase
    {

        private readonly IAdminService _adminService;
        private readonly IDepartmentService _departmentService;
        private readonly IUserService _userService;
        private readonly IEmployeeService _employeeService;
        

        public AdminController(IAdminService adminService, IDepartmentService departmentService,IUserService userService,IEmployeeService employeeService)
        {
            _adminService = adminService;
            _departmentService = departmentService;
            _userService= userService;
            _employeeService = employeeService;
        }


        ////////////////////////////////////----------------------------Employees----------------------------//////////////////////////////////////////////////////

        [HttpGet("employees")]
        public async Task<ActionResult<IEnumerable<EmployeeModel>>> GetAllEmployees()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }


        [HttpGet("employees/{id}")]
        public async Task<ActionResult<EmployeeModel>> GetEmployeeById(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }


        [HttpPost("createEmployee")]
        public async Task<IActionResult> CreateEmployee([FromBody] EmployeeModel newEmployee)
        {
            if (newEmployee == null)
            {
                return BadRequest("Employee data is required.");
            }

            // If EmployeeImage contains a data URI prefix (e.g., "data:image/png;base64,..."), remove it
            if (!string.IsNullOrEmpty(newEmployee.EmployeeImage) && newEmployee.EmployeeImage.Contains(","))
            {
                var base64Data = newEmployee.EmployeeImage.Split(',')[1];  // Extract base64 part
                newEmployee.EmployeeImage = base64Data;
            }

            var isCreated = await _employeeService.CreateEmployeeAsync(newEmployee);

            if (isCreated)
            {
                return Ok(new { message = "Employee created successfully." });
            }

            return StatusCode(500, "An error occurred while creating the employee.");
        }


        // PUT: api/admin/updateEmployee
        [HttpPut("updateEmployee")]
        public async Task<IActionResult> UpdateEmployee([FromBody] EmployeeModel employee)
        {
            if (employee == null || employee.EmployeeId == 0)
            {
                return BadRequest("Employee data is required.");
            }

            // Handle EmployeeImage if it's a Data URI (e.g., "data:image/png;base64,...")
            if (!string.IsNullOrEmpty(employee.EmployeeImage) && employee.EmployeeImage.Contains(","))
            {
                var base64Data = employee.EmployeeImage.Split(',')[1];  // Extract the base64 part
                employee.EmployeeImage = base64Data;
            }

            // Call the service method to update the employee
            var isUpdated = await _employeeService.UpdateEmployeeAsync(employee);

            if (isUpdated)
            {
                return Ok(new { message = "Employee updated successfully." });
            }

            return StatusCode(500, "An error occurred while updating the employee.");
        }


        //////////////////////////-------------------------Users----------------------------//////////////////////////////////////////////////////////////////////

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<Users>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPut("users/{userId}/activate")]
        public async Task<IActionResult> UpdateUserIsActiveStatus(int userId, [FromBody] UserStatusUpdateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid request payload." });
            }

            var result = await _userService.UpdateUserIsActiveStatus(userId, request.IsActive);
            if (result)
            {
                return Ok(new { message = "User IsActive status updated successfully." });
            }
            return BadRequest(new { message = "Failed to update User IsActive status." });
        }


        [HttpGet("users/role/{roleId}")]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsersByRoleId(int roleId)
        {
            var users = await _userService.GetUsersByRoleIdAsync(roleId);
            return Ok(users);
        }




        //////////////////////////-------------------------Departments----------------------------//////////////////////////////////////////////////////////////////////



        
        [HttpGet("departments")]
        public async Task<ActionResult<IEnumerable<DepartmentModel>>> GetDepartments()
        {
            var departments = await _departmentService.GetDepartmentsAsync();
            return Ok(departments);
        }

     
        [HttpGet("departments/{id}")]
        public async Task<ActionResult<DepartmentModel>> GetDepartmentById(int id)
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return Ok(department);
        }

       
        [HttpPost("departments")]
        public async Task<IActionResult> AddDepartment([FromBody] DepartmentModel department)
        {
            department.CreatedAt = DateTime.UtcNow;
            var isAdded = await _departmentService.AddDepartmentAsync(department);

            if (isAdded)
            {
                return Ok(new { message = "Department added successfully." });
            }

            return StatusCode(500, "An error occurred while adding the department.");
        }


        
        [HttpPut("departments")]
        public async Task<IActionResult> UpdateDepartment([FromBody] DepartmentModel department)
        {
            var isUpdated = await _departmentService.UpdateDepartmentAsync(department);

            if (isUpdated)
            {
                return Ok(new { message = "Department updated successfully." });
            }

            return StatusCode(500, "An error occurred while updating the department.");
        }

    }
}
