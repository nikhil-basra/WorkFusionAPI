using Microsoft.AspNetCore.Mvc;
using WorkFusionAPI.Interfaces;
using WorkFusionAPI.Models;

namespace WorkFusionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForgotPasswordController : ControllerBase
    {
        private readonly IForgotPasswordService _forgotPasswordService;

        public ForgotPasswordController(IForgotPasswordService forgotPasswordService)
        {
            _forgotPasswordService = forgotPasswordService;
        }

        [HttpPost("SendOTP")]
        public async Task<IActionResult> SendOTP([FromBody] ForgotPasswordModel model)
        {
            if (string.IsNullOrEmpty(model.Email))
                return BadRequest("Email is required.");

            bool result = await _forgotPasswordService.SendOTP(model.Email);
            if (!result)
                return NotFound(new { message = "Email not found or inactive." });

            return Ok(new { message = "OTP sent successfully." });
        }


        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.OTP) || string.IsNullOrEmpty(model.NewPassword))
            {
                return BadRequest(new { message = "Email, OTP, and new password are required." });
            }

            bool result = await _forgotPasswordService.ResetPassword(model.Email, model.OTP, model.NewPassword);
            if (!result)
            {
                return BadRequest(new { message = "Invalid OTP or expired." });
            }

            return Ok(new { message = "Password reset successfully." });
        }

    }
}
