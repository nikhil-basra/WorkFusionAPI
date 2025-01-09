using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkFusionAPI.Interfaces;
using WorkFusionAPI.Models;
using WorkFusionAPI.Services;

namespace WorkFusionAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "3")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IImageService _imageService;
        private readonly IDepartmentService _departmentService;
        private readonly IUserService _userService;
        private readonly ITaskService _taskService;
        private readonly IProjectsService _projectsService;
        private readonly INotificationService _notificationService;
        private readonly ILeaveService _leaveService;

        public EmployeeController(
            IEmployeeService employeeService,
            IImageService imageService,
            IDepartmentService departmentService,
            IUserService userService,
            ITaskService taskService,
            IProjectsService projectsService,
            INotificationService notificationService,
            ILeaveService leaveService)
        {
            _employeeService = employeeService;
            _imageService = imageService;
            _departmentService = departmentService;
            _userService = userService;
            _taskService = taskService;
            _projectsService = projectsService;
            _notificationService = notificationService;
            _leaveService = leaveService;
        }

        //-------------------------------------employee--------------------------------//

        [HttpGet("{userId}")]
        public async Task<ActionResult<EmployeeModel>> GetEmployeeByUserId(int userId)
        {
            var employee = await _employeeService.GetEmployeeByUserIdAsync(userId);
            if (employee == null)
            {
                return NotFound(); // Return 404 if employee not found
            }
            return Ok(employee); 
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


        //------------------------------------------Departments-------------------------------//

        [HttpGet("departments")]
        public async Task<ActionResult<IEnumerable<DepartmentModel>>> GetDepartments()
        {
            var departments = await _departmentService.GetDepartmentsAsync();
            return Ok(departments);
        }


        //------------------------------------------------get image ----------------------------------------------//

        [HttpGet("employeeImage/{userId}/{roleId}")]
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


        //------------------tasks---------------------------------------------//
        [HttpGet("getTaskbyEmployeeId/{employeeId}")]
        public async Task<IActionResult> GetTaskByEmployeeId(int employeeId)
        {
            try
            {
                var tasks = await _taskService.GetTaskByEmployeeId(employeeId);
                if (tasks == null || !tasks.Any())
                    return NotFound("No tasks found for the given managerId.");

                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("updateTasksStatus")]
        public async Task<IActionResult> UpdateTaskStatus([FromBody] TaskStatusModel task)
        {
            try
            {
                var result = await _taskService.UpdateTaskStatus(task);
                return Ok(new { success = result > 0 });
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

        //----------------projects-----------------------

        // Get projects by employee ID
        [HttpGet("GetProjectsByEmployeeId/{employeeId}")]
        public async Task<IActionResult> GetProjectsByEmployeeId(int employeeId)
        {
            var projects = await _projectsService.GetProjectsByEmployeeIdAsync(employeeId);

            if (projects == null || !projects.Any())
                return NotFound("No projects found for this employee.");

            return Ok(projects);
        }



        //---------graphs-------------------------

        [HttpGet("TasksCounts/{employeeId}")]
        public async Task<IActionResult> GetTaskCounts(int employeeId)
        {
            try
            {
                var counts = await _taskService.GetTaskCountsAsync(employeeId);
                return Ok(counts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving task counts.", Details = ex.Message });
            }
        }




        //------------------------------notifications-------------------------------
        [HttpGet("GetNotificationByEntityId/{entityId}/RoleId/{roleId}")]
        public async Task<IActionResult> GetNotificationsByEntityAndRole(int entityId, int roleId)
        {
            var notifications = await _notificationService.GetNotificationsByEntityAndRole(entityId, roleId);
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


        //-----------------Leaves-------------------

        //get leave requests list for employee
        [HttpGet("GetEmployeeLeaves/{employeeId}")]
        public async Task<IActionResult> GetEmployeeLeaves(int employeeId)
        {
            try
            {
                var leaves = await _leaveService.GetLeavesByEmployeeIdAsync(employeeId);
                if (leaves == null || !leaves.Any())
                {
                    return NotFound("No leaves found for the employee.");
                }
                return Ok(leaves);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        //Submit leaverequest
        [HttpPost("submit-leave-request")]
        public async Task<IActionResult> SubmitLeaveRequest([FromBody] LeaveModel leaveRequest)
        {
            try
            {
                var result = await _leaveService.SubmitLeaveRequestAsync(leaveRequest);

                if (result)
                {
                    return Ok(new { message = "Leave request submitted successfully." });
                }
                else
                {
                    return BadRequest(new { message = "Failed to submit leave request." });
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Directly returning the error response without logging.
                return StatusCode(500, new { message = "An internal error occurred." });
            }
        }


    }
}
