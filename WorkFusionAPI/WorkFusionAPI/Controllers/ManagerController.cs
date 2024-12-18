using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WorkFusionAPI.Interfaces;
using WorkFusionAPI.Models;
using WorkFusionAPI.Services;

namespace WorkFusionAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "2")]
    public class ManagerController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly IUserService _userService;
        private readonly IManagerService _managerService;
        private readonly IProjectsService _projectsService;
        private readonly IClientsProjectRequestsService _clientsProjectRequestsService;
        private readonly IClientService _clientService;
        private readonly IEmployeeService _employeeService;
        private readonly ITaskService _taskService;

        public ManagerController(
            IImageService imageService,
            IUserService userService,
            IManagerService managerService,
            IProjectsService projectsService,
            IClientsProjectRequestsService clientsProjectRequestsService,
             IClientService clientService,
             IEmployeeService employeeService,
             ITaskService taskService
            )
        {
            _imageService = imageService;
            _userService = userService;
            _managerService = managerService;
            _projectsService = projectsService;
            _clientsProjectRequestsService = clientsProjectRequestsService;
            _clientService = clientService;
            _employeeService = employeeService;
            _taskService = taskService;
        }

        //-------------------------------------------------employee--------------------------------
        [HttpGet("GetByManager/{managerId}")]
        public async Task<IActionResult> GetEmployeesByManagerId(int managerId)
        {
            try
            {
                var employees = await _employeeService.GetEmployeesByManagerIdAsync(managerId);
                return Ok(employees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
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


        //--------------------get image --------------------------//
        [HttpGet("managerImage/{userId}/{roleId}")]
        public async Task<ActionResult<UserImageModel>> GetImage(int userId, int roleId)
        {
            var base64Image = await _imageService.GetImageByUserIdAndRoleIdAsync(userId, roleId);

            if (string.IsNullOrEmpty(base64Image))
            {
                return NotFound("Image not found for the given user and role.");
            }

            return Ok(new UserImageModel { Base64Image = base64Image });
        }

        //----------------------------reset password------------------------------//
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(int userId, string username, string currentPassword, string newPassword)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
                {
                    return BadRequest("Invalid input. All fields are required.");
                }

                // Verify user credentials
                var userValid = await _userService.VerifyUserCredentials(userId, username, currentPassword);
                if (!userValid)
                {
                    return Unauthorized("Invalid username or password.");
                }

                // Reset the password
                var result = await _userService.ResetPassword(userId, newPassword);
                if (result)
                {
                    return Ok("Password reset successfully.");
                }

                return StatusCode(500, "Error occurred while resetting the password.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        //----------------------------manager-------------------------------//

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

        //-------------------------get projects------------------------------//

        [HttpGet("projectsRequests")]
        public async Task<ActionResult<IEnumerable<ClientsProjectRequestsModel>>> GetAllProjectsRequests()
        {
            var projects = await _clientsProjectRequestsService.GetAllProjectsRequestsAsync();
            return Ok(projects);
        }


        [HttpGet("projectsRequests/{projectrequestId}")]
        public async Task<ActionResult<ClientsProjectRequestsModel>> GetProjectRequestsById(int projectrequestId)
        {
            var project = await _clientsProjectRequestsService.GetProjectRequestsByIdAsync(projectrequestId);
            if (project == null)
            {
                return NotFound();
            }
            return Ok(project);
        }


        [HttpPost("projectsRequests")]
        public async Task<ActionResult> AddProjectRequest([FromBody] ClientsProjectRequestsModel projectRequest)
        {
            if (await _clientsProjectRequestsService.AddProjectRequestAsync(projectRequest))
            {
                return CreatedAtAction(nameof(GetProjectRequestsById), new { projectrequestId = projectRequest.ProjectRequestID }, projectRequest);
            }
            return BadRequest("Failed to create the project request.");
        }


        [HttpPut("projectsRequests/{projectrequestId}")]
        public async Task<ActionResult> UpdateProjectRequest(int projectrequestId, [FromBody] ClientsProjectRequestsModel projectRequest)
        {
          
            if (projectrequestId != projectRequest.ProjectRequestID)
            {
                return BadRequest("Project ID mismatch.");
            }

            bool isUpdated = await _clientsProjectRequestsService.UpdateProjectRequestAsync(projectRequest);

            if (isUpdated)
            {
                return Ok(); // Return 204 No Content if successful
            }

            return Ok(); // Return 404 Not Found if the project does not exist
        }


        [HttpDelete("projectsRequests/{projectrequestId}")]
        public async Task<ActionResult> DeleteProjectRequest(int projectrequestId)
        {
            if (await _clientsProjectRequestsService.DeleteProjectRequestAsync(projectrequestId))
            {
                return NoContent();
            }
            return NotFound();
        }



        [HttpGet("projects")]
        public async Task<ActionResult<List<ProjectsModel>>> GetAllProjects()
        {
            var projects = await _projectsService.GetAllProjectsAsync();
            return Ok(projects);
        }


        [HttpGet("projects/{id}")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            var project = await _projectsService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            return Ok(project);
        }

        [HttpGet("manager/{managerId}")]
        public async Task<IActionResult> GetProjectsByManager(int managerId)
        {
            var projects = await _projectsService.GetProjectsByManagerIdAsync(managerId);

            if (projects == null || !projects.Any())
                return NotFound("No projects found for this manager.");

            return Ok(projects);
        }


        [HttpPost("projects")]
        public async Task<ActionResult<int>> CreateProject([FromBody] ProjectsModel project)
        {
            if (project == null)
            {
                return BadRequest("Project data is null.");
            }

            var projectId = await _projectsService.CreateProjectAsync(project);
     
            return Ok(projectId);
        }


        [HttpPut("projects/{id}")]
        public async Task<IActionResult> UpdateProject([FromBody] ProjectsModel project)
        {  
            await _projectsService.UpdateProjectAsync(project);
            return Ok();
        }


        [HttpGet("project-requests/{managerId}")]
        public async Task<IActionResult> GetProjectRequestsByManager(int managerId)
        {
            var projectRequests = await _clientsProjectRequestsService.GetProjectRequestsByManagerAsync(managerId);

            if (projectRequests == null || !projectRequests.Any())
            {
                return NotFound("No project requests found for this manager.");
            }

            return Ok(projectRequests);
        }


        [HttpDelete("projects/{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var existingProject = await _projectsService.GetProjectByIdAsync(id);
            if (existingProject == null)
            {
                return NotFound();
            }

            await _projectsService.DeleteProjectAsync(id);
            return NoContent();
        }

        //------------------------------------clients--------------------------------------------//

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

        //-------------------------------tasks--------------------------------------//

        [HttpPost("addtask")]
        public async Task<IActionResult> CreateTask([FromBody] TaskModel task)
        {
            try
            {
                var result = await _taskService.CreateTask(task);
                return Ok(new { success = result > 0 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getTaskbyManagerId/{managerId}")]
        public async Task<IActionResult> GetTaskByManagerId(int managerId)
        {
            try
            {
                var tasks = await _taskService.GetTaskByManagerId(managerId);
                if (tasks == null || !tasks.Any())
                    return NotFound("No tasks found for the given managerId.");

                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet("getAllTasks")]
        public async Task<IActionResult> GetAllTasks()
        {
            try
            {
                var tasks = await _taskService.GetAllTasks();
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet("getTaskbyId/{taskId}")]
        public async Task<IActionResult> GetTaskById(int taskId)
        {
            try
            {
                var task = await _taskService.GetTaskById(taskId);
                if (task == null) return NotFound();
                return Ok(task);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("updateTasks")]
        public async Task<IActionResult> UpdateTask([FromBody] TaskModel task)
        {
            try
            {
                var result = await _taskService.UpdateTask(task);
                return Ok(new { success = result > 0 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("deleteTasks/{taskId}")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            try
            {
                var result = await _taskService.DeleteTask(taskId);
                return Ok(new { success = result > 0 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
