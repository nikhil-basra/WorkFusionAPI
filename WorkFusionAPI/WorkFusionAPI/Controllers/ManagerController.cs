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
        private readonly INotificationService _notificationService;
        private readonly ILeaveService _leaveService;

        public ManagerController(
            IImageService imageService,
            IUserService userService,
            IManagerService managerService,
            IProjectsService projectsService,
            IClientsProjectRequestsService clientsProjectRequestsService,
             IClientService clientService,
             IEmployeeService employeeService,
             ITaskService taskService,
             INotificationService notificationService,
             ILeaveService leaveService
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
            _notificationService = notificationService;
            _leaveService = leaveService;
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


        [HttpGet("manager/{managerId}/project-status-counts")]
        public async Task<IActionResult> GetProjectStatusCounts(int managerId)
        {
            var counts = await _projectsService.GetProjectStatusCountsByManagerIdAsync(managerId);

            if (counts == null)
                return NotFound("No project data found for this manager.");

            return Ok(counts);
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
                if (task == null)
                    return NotFound("Task not found for the given taskId.");

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



        //------------------------------notifications-------------------------------
        [HttpGet("GetNotificationByEntityId/{entityId}/RoleId/{roleId}")]
        public async Task<IActionResult> GetNotificationsByEntityAndRole(int entityId, int roleId)
        {
            var notifications = await _notificationService.GetNotificationsByEntityAndRole(entityId, roleId);
            return Ok(notifications);
        }


        [HttpPost]
        public async Task<IActionResult> CreateNotification(NotificationModel notification)
        {
            await _notificationService.AddNotification(notification);
            return Ok(new { Message = "Notification created successfully" });
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetNotifications(int userId)
        {
            var notifications = await _notificationService.GetNotificationsByUserId(userId);
            return Ok(notifications);
        }


        [HttpPut("markRead/{notificationId}")]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            await _notificationService.MarkAsRead(notificationId);
            return Ok(new { Message = "Notification marked as read" });
        }


        [HttpDelete("DeleteNotification/{notificationId}")]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            await _notificationService.DeleteNotification(notificationId);
            return Ok(new { Message = "Notification deleted successfully" });
        }


        [HttpGet("CountUnreadNotification/{entityId}/{roleId}")]
        public async Task<IActionResult> CountUnreadNotifications(int entityId, int roleId)
        {
            try
            {
                var count = await _notificationService.CountUnreadNotificationsByEntityAndRole(entityId, roleId);
                return Ok(new { UnreadCount = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }


        //----------------------Leaves Approval--------------------------------


        //Get Leaverequests for manager
        [HttpGet("GetLeaveRequestsByManager/{managerId}")]
        public async Task<IActionResult> GetLeaveRequestsByManager(int managerId)
        {
            var leaveRequests = await _leaveService.GetLeaveRequestsByManagerAsync(managerId);

            if (leaveRequests == null || !leaveRequests.Any())
            {
                return NotFound("No leave requests found for this manager.");
            }

            return Ok(leaveRequests);
        }

        //Get Pending leave requests list
        [HttpGet("manager/{managerId}/pending")]
        public async Task<IActionResult> GetPendingLeaveRequestsForManager(int managerId)
        {
            try
            {
                var leaveRequests = await _leaveService.GetPendingLeaveRequestsByManagerAsync(managerId);
                if (!leaveRequests.Any())
                {
                    return NotFound("No pending leave requests found for this manager.");
                }

                return Ok(leaveRequests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //Get Rejected Leave Requests List
        [HttpGet("manager/{managerId}/rejected")]
        public async Task<IActionResult> GetRejectedLeaveRequestsForManager(int managerId)
        {
            try
            {
                var leaveRequests = await _leaveService.GetRejectedLeaveRequestsByManagerAsync(managerId);
                if (!leaveRequests.Any())
                {
                    return NotFound("No rejected leave requests found for this manager.");
                }

                return Ok(leaveRequests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //approve leave requests
        [HttpGet("approved-leave-requests/{managerId}")]
        public async Task<IActionResult> GetApprovedLeaveRequests(int managerId)
        {
            try
            {
                var approvedLeaves = await _leaveService.GetApprovedLeaveRequestsByManagerAsync(managerId);

                if (approvedLeaves == null || !approvedLeaves.Any())
                {
                    return NotFound("No approved leave requests found for this manager.");
                }

                return Ok(approvedLeaves);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching approved leave requests: {ex.Message}");
            }
        }


        // Accept leave request
        [HttpPost("leave-requests/{leaveId}/accept")]
        public async Task<IActionResult> AcceptLeaveRequest(int leaveId, [FromQuery] int? managerId)
        {
            if (!managerId.HasValue)
            {
                return BadRequest(new { message = "ManagerId cannot be null." });
            }

            var result = await _leaveService.AcceptLeaveRequestAsync(leaveId, managerId.Value);

            if (result)
            {
                return Ok(new { message = "Leave request approved successfully." });
            }

            return NotFound(new { message = "Leave request not found or already processed." });
        }


        //Reject leave request
        [HttpPost("leave-requests/{leaveId}/reject")]
        public async Task<IActionResult> RejectLeaveRequest(int leaveId, [FromQuery] int? managerId)
        {
            if (!managerId.HasValue)
            {
                return BadRequest("ManagerId cannot be null.");
            }

            var result = await _leaveService.RejectLeaveRequestAsync(leaveId, managerId.Value);

            if (result)
            {
                return Ok(new { message = "Leave request rejected successfully." });
            }

            return NotFound(new { message = "Leave request not found or already processed." });
        }

    }
}
