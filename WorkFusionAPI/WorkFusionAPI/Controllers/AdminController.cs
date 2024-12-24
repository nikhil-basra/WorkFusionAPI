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
        private readonly IManagerService _managerService;
        private readonly IClientService _clientService;
        private readonly IImageService _imageService;
        private readonly IProjectsService _projectsService;

        public AdminController(
            IAdminService adminService, 
            IDepartmentService departmentService,
            IUserService userService,
            IEmployeeService employeeService,
            IManagerService managerService,
            IClientService clientService,
            IImageService imageService,
            IProjectsService projectsService)
        {
            _adminService = adminService;
            _departmentService = departmentService;
            _userService = userService;
            _employeeService = employeeService;
            _managerService = managerService;
            _clientService = clientService;
            _imageService = imageService;
            _projectsService = projectsService;
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


        //////////////////************************Manager*************************////////////////////////////////////////////////////////


        [HttpGet("managers")]
        public async Task<ActionResult<IEnumerable<ManagerModel>>> GetAllManagers()
        {
            var managers = await _managerService.GetAllManagersAsync();
            return Ok(managers);
        }

        [HttpGet("managers/{id}")]
        public async Task<ActionResult<ManagerModel>> GetManagerById(int id)
        {
            var manager = await _managerService.GetManagerByIdAsync(id);
            if (manager == null)
            {
                return NotFound();
            }
            return Ok(manager);
        }

        [HttpPost("createManager")]
        public async Task<IActionResult> CreateManager([FromBody] ManagerModel newManager)
        {
            if (newManager == null)
            {
                return BadRequest("Manager data is required.");
            }

            var isCreated = await _managerService.CreateManagerAsync(newManager);

            if (isCreated)
            {
                return Ok(new { message = "Manager created successfully." });
            }

            return StatusCode(500, "An error occurred while creating the manager.");
        }

        [HttpPut("updateManager")]
        public async Task<IActionResult> UpdateManager([FromBody] ManagerModel manager)
        {
            if (manager == null || manager.ManagerId == 0)
            {
                return BadRequest("Manager data is required.");
            }

            var isUpdated = await _managerService.UpdateManagerAsync(manager);

            if (isUpdated)
            {
                return Ok(new { message = "Manager updated successfully." });
            }

            return StatusCode(500, "An error occurred while updating the manager.");
        }

        [HttpDelete("managers/{id}")]
        public async Task<IActionResult> DeleteManager(int id)
        {
            var isDeleted = await _managerService.DeleteManagerAsync(id);
            if (isDeleted)
            {
                return Ok(new { message = "Manager deleted successfully." });
            }

            return StatusCode(500, "An error occurred while deleting the manager.");
        }


        //***************************************Client***********************************************//

        [HttpGet("clients")]
        public async Task<ActionResult<IEnumerable<ClientModel>>> GetAllClients()
        {
            var clients = await _clientService.GetAllClientsAsync();
            return Ok(clients);
        }

        [HttpGet("clients/{id}")]
        public async Task<ActionResult<ClientModel>> GetClientById(int id)
        {
            var client = await _clientService.GetClientByIdAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            return Ok(client);
        }

        [HttpPost("createClient")]
        public async Task<IActionResult> CreateClient([FromBody] ClientModel newClient)
        {
            if (newClient == null)
            {
                return BadRequest("Client data is required.");
            }

            var isCreated = await _clientService.CreateClientAsync(newClient);

            if (isCreated)
            {
                return Ok(new { message = "Client created successfully." });
            }

            return StatusCode(500, "An error occurred while creating the client.");
        }

        [HttpPut("updateClient")]
        public async Task<IActionResult> UpdateClient([FromBody] ClientModel client)
        {
            if (client == null || client.ClientId == 0)
            {
                return BadRequest("Client data is required.");
            }

            var isUpdated = await _clientService.UpdateClientAsync(client);

            if (isUpdated)
            {
                return Ok(new { message = "Client updated successfully." });
            }

            return StatusCode(500, "An error occurred while updating the client.");
        }

        [HttpDelete("clients/{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var isDeleted = await _clientService.DeleteClientAsync(id);
            if (isDeleted)
            {
                return Ok(new { message = "Client deleted successfully." });
            }

            return StatusCode(500, "An error occurred while deleting the client.");
        }


        //-----------------------------admins----------------------------------------//
        [HttpGet("admin/{userId}")]
        public async Task<IActionResult> GetAdminByUserId(int userId)
        {
            var admin = await _adminService.GetAdminByUserIdAsync(userId);
            if (admin == null) return NotFound();
            return Ok(admin);
        }


        [HttpPut("admin/{userId}")]
        public async Task<IActionResult> UpdateAdminByUserId(int userId, [FromBody] AdminModel admin)
        {
            if (userId != admin.UserId) return BadRequest("User ID mismatch.");

            var result = await _adminService.UpdateAdminByUserIdAsync(userId, admin);

            if (!result) return BadRequest("Failed to update admin.");

            return Ok(new { message = "Admin updated successfully." });
        }

        //------------------------------------------------get image ----------------------------------------------//

        [HttpGet("adminImage/{userId}/{roleId}")]
        public async Task<ActionResult<UserImageModel>> GetImage(int userId, int roleId)
        {
            var base64Image = await _imageService.GetImageByUserIdAndRoleIdAsync(userId, roleId);

            if (string.IsNullOrEmpty(base64Image))
            {
                return NotFound("Image not found for the given user and role.");
            }

            return Ok(new UserImageModel { Base64Image = base64Image });
        }

        //----------------------------------projects------------------------------------------

        [HttpGet("projects")]
        public async Task<ActionResult<List<ProjectsModel>>> GetAllProjects()
        {
            var projects = await _projectsService.GetAllProjectsAsync();
            return Ok(projects);
        }



        //------------------------------graphs-------------------------------------------

        [HttpGet("All-projects-status-counts")]
        public async Task<IActionResult> GetProjectStatusCounts()
        {
            try
            {
                var counts = await _projectsService.GetProjectStatusCountsAsync();
                return Ok(counts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching project status counts.", Details = ex.Message });
            }
        }

        [HttpGet("department-project-status-counts")]
        public async Task<IActionResult> GetDepartmentProjectStatusCounts()
        {
            var result = await _projectsService.GetDepartmentProjectStatusCountsAsync();
            return Ok(result);
        }

        [HttpGet("active-employee-counts")]
        public async Task<IActionResult> GetActiveEmployeeCounts()
        {
            var result = await _departmentService.GetActiveEmployeeCountsAsync();
            return Ok(result);
        }

    }
}
