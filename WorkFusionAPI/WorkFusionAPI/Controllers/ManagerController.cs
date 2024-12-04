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

        public ManagerController(IImageService imageService, IUserService userService, IManagerService managerService )
        {
              _imageService = imageService;
            _userService = userService;
            _managerService = managerService;

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

    }
}
