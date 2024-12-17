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
    [Authorize(Roles = "4")] // Restrict access to Client role
    public class ClientController : ControllerBase
    {
        private readonly IClientsProjectRequestsService _clientsProjectRequestsService;
        private readonly IDepartmentService _departmentService;
        private readonly IImageService _imageService;
        private readonly IClientService _clientService;
        private readonly IUserService _userService;
        private readonly IProjectsService _projectsService;

        public ClientController(
            IClientsProjectRequestsService clientsProjectRequestsService,
            IUserService userService,
            IDepartmentService departmentService, 
            IImageService imageService,
            IClientService clientService,
            IProjectsService projectsService
            )
        {
            _clientsProjectRequestsService = clientsProjectRequestsService;
            _departmentService = departmentService;
            _imageService = imageService;
            _clientService = clientService;
            _userService = userService;
            _projectsService = projectsService;
        }

        //-------------------------------client----------------------------------------//
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



        //------------------------------ClientsProjectsRequests-----------------------------------------------//

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

            if (await _clientsProjectRequestsService.UpdateProjectRequestAsync(projectRequest))
            {
                return NoContent();
            }
            return NotFound();
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

        //-------------------------------------------------------departments---------------------------------------------------//

        [HttpGet("departments")]
        public async Task<ActionResult<IEnumerable<DepartmentModel>>> GetDepartments()
        {
            var departments = await _departmentService.GetDepartmentsAsync();
            return Ok(departments);
        }

        //------------------------------------------------get image ------------------------------------------------------

        [HttpGet("clientImage/{userId}/{roleId}")]
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

        //-----------------------projects ---------------------------
        [HttpGet("projects/{clientId}")]
        public async Task<ActionResult<IEnumerable<ProjectsModel>>> GetClientProjects(int clientId)
        {
            var projects = await _projectsService.GetProjectsByClientIdAsync(clientId);

            if (projects == null || !projects.Any())
            {
                return NotFound("No projects found for the client.");
            }

            return Ok(projects);
        }
    }
}
