using Dapper;
using MailKit.Net.Smtp;
using MimeKit;
using System.Security.Cryptography;
using System.Text;
using WorkFusionAPI.Interfaces;
using WorkFusionAPI.Models;
using WorkFusionAPI.Utility;

namespace WorkFusionAPI.Services
{
    public class ForgotPasswordService : IForgotPasswordService
    {
        private readonly DBGateway _dbGateway;
        private readonly IConfiguration _configuration;

        public ForgotPasswordService(DBGateway dbGateway, IConfiguration configuration)
        {
            _dbGateway = dbGateway;
            _configuration = configuration;
        }

        public async Task<bool> SendOTP(string email)
        {
            // Check if email exists in the Users table
            string userQuery = "SELECT UserId FROM Users WHERE Email = @Email AND IsActive = 1";
            var parameters = new DynamicParameters();
            parameters.Add("@Email", email);

            var userId = await _dbGateway.ExeScalarQuery<int?>(userQuery, parameters);

            if (userId == null)
                return false;

            // Generate OTP
            string otp = GenerateOTP();
            DateTime expiryTime = DateTime.UtcNow.AddMinutes(5); // OTP valid for 5 minutes

            // Save OTP to the database
            string otpInsertQuery = "INSERT INTO OTPStore (UserId, OTP, ExpiryTime, IsUsed) VALUES (@UserId, @OTP, @ExpiryTime, 0)";
            parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@OTP", otp);
            parameters.Add("@ExpiryTime", expiryTime);

            await _dbGateway.ExeQuery(otpInsertQuery, parameters);

            // Send OTP via email
            await SendEmail(email, otp);

            return true;
        }

        public async Task<bool> ResetPassword(string email, string otp, string newPassword)
        {
            // Validate OTP
            string otpQuery = @"
                SELECT UserId FROM OTPStore 
                WHERE OTP = @OTP AND ExpiryTime > @Now AND IsUsed = 0";

            var parameters = new DynamicParameters();
            parameters.Add("@OTP", otp);
            parameters.Add("@Now", DateTime.UtcNow);

            var userId = await _dbGateway.ExeScalarQuery<int?>(otpQuery, parameters);

            if (userId == null)
                return false;

            // Hash the new password using SHA512
            string hashedPassword = HashPassword(newPassword);

            // Update the password in the Users table
            string updatePasswordQuery = "UPDATE Users SET PasswordHash = @PasswordHash WHERE UserId = @UserId";
            parameters = new DynamicParameters();
            parameters.Add("@PasswordHash", hashedPassword);
            parameters.Add("@UserId", userId);

            await _dbGateway.ExeQuery(updatePasswordQuery, parameters);

            // Mark OTP as used
            string updateOtpQuery = "UPDATE OTPStore SET IsUsed = 1 WHERE OTP = @OTP";
            parameters = new DynamicParameters();
            parameters.Add("@OTP", otp);

            await _dbGateway.ExeQuery(updateOtpQuery, parameters);

            return true;
        }

        private string GenerateOTP()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[4];
                rng.GetBytes(data);
                int otp = BitConverter.ToInt32(data, 0) % 1000000;
                return Math.Abs(otp).ToString("D6");
            }
        }

        // SHA512 Password Hashing
        private string HashPassword(string password)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha512.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private async Task SendEmail(string toEmail, string otp)
        {
            if (string.IsNullOrEmpty(toEmail))
            {
                throw new ArgumentException("Recipient email address is required.", nameof(toEmail));
            }

            // Retrieve email configuration
            string senderEmail = _configuration["Email:Sender"];
            string senderPassword = _configuration["Email:Password"];
            string smtpServer = _configuration["Email:SmtpServer"];
            string smtpPortString = _configuration["Email:Port"];

            if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword) ||
                string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpPortString))
            {
                throw new InvalidOperationException("Email configuration is not properly set in appsettings.json.");
            }

            // Safely parse the SMTP port
            if (!int.TryParse(smtpPortString, out int smtpPort))
            {
                throw new FormatException("The SMTP port in the configuration is not a valid number.");
            }

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("WorkFusion Support", senderEmail));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = "Your OTP Code";
            email.Body = new TextPart("plain")
            {
                Text = $"Your OTP code is: {otp}. It will expire in 5 minutes."
            };

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(smtpServer, smtpPort, true);
                await smtp.AuthenticateAsync(senderEmail, senderPassword);
                await smtp.SendAsync(email);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while sending the email.", ex);
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
    }
}
