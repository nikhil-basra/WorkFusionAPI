using Microsoft.AspNetCore.Mvc;
using WorkFusionAPI.Interfaces;
using WorkFusionAPI.Models;

namespace WorkFusionAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // Remove [Authorize] to allow access without authentication
        [HttpGet("{userId}")]
        public async Task<ActionResult<EmployeeModel>> GetEmployeeByUserId(int userId)
        {
            var employee = await _employeeService.GetEmployeeByUserIdAsync(userId);
            if (employee == null)
            {
                return NotFound(); // Return 404 if employee not found
            }
            return Ok(employee); // Return employee details if found
        }

        //[HttpPut]
        //public async Task<IActionResult> UpdateEmployee([FromBody] EmployeeModel employee)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState); // Return 400 Bad Request if model state is invalid
        //    }

        //    var result = await _employeeService.UpdateEmployeeAsync(employee);
        //    if (!result)
        //    {
        //        return NotFound(); // Return 404 if update fails
        //    }

        //    return NoContent(); // Return 204 No Content on successful update
        //}
    }
}
