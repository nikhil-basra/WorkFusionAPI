using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkFusionAPI.Interfaces;
using WorkFusionAPI.Models;

namespace WorkFusionAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "4")] // Restrict access to Client role
    public class ClientController : ControllerBase
    {
        private readonly IClientsProjectRequestsService _clientsProjectRequestsService;

        public ClientController(IClientsProjectRequestsService clientsProjectRequestsService)
        {
            _clientsProjectRequestsService = clientsProjectRequestsService;
        }

        [HttpGet("projectsRequests")]
        public async Task<ActionResult<IEnumerable<ClientsProjectRequestsModel>>> GetAllProjects()
        {
            var projects = await _clientsProjectRequestsService.GetAllProjectsAsync();
            return Ok(projects);
        }

        [HttpGet("projectsRequests/{projectId}")]
        public async Task<ActionResult<ClientsProjectRequestsModel>> GetProjectById(int projectId)
        {
            var project = await _clientsProjectRequestsService.GetProjectByIdAsync(projectId);
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
                return CreatedAtAction(nameof(GetProjectById), new { projectId = projectRequest.ProjectID }, projectRequest);
            }
            return BadRequest("Failed to create the project request.");
        }

        [HttpPut("projectsRequests/{projectId}")]
        public async Task<ActionResult> UpdateProjectRequest(int projectId, [FromBody] ClientsProjectRequestsModel projectRequest)
        {
            if (projectId != projectRequest.ProjectID)
            {
                return BadRequest("Project ID mismatch.");
            }

            if (await _clientsProjectRequestsService.UpdateProjectRequestAsync(projectRequest))
            {
                return NoContent();
            }
            return NotFound();
        }

        [HttpDelete("projectsRequests/{projectId}")]
        public async Task<ActionResult> DeleteProjectRequest(int projectId)
        {
            if (await _clientsProjectRequestsService.DeleteProjectRequestAsync(projectId))
            {
                return NoContent();
            }
            return NotFound();
        }
    }
}
