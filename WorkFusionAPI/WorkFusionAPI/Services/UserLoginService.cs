using Dapper;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WorkFusionAPI.Interfaces;
using WorkFusionAPI.Models;
using WorkFusionAPI.Utility;

namespace WorkFusionAPI.Services
{
    public class UserLoginService : IUserLoginService
    {
        private readonly DBGateway _dbGateway;
        private readonly IConfiguration _configuration;

        public UserLoginService(DBGateway dbGateway, IConfiguration configuration)
        {
            _dbGateway = dbGateway;
            _configuration = configuration;
        }

        public async Task<string> Authenticate(UserLoginModel loginModel)
        {
            // Hash the provided password using SHA-512
            string hashedPassword = HashPassword(loginModel.Password);

            // Query to check if the RoleId, Username, and PasswordHash match
            string query = @"
                SELECT UserId, Username, RoleId 
                FROM UserLogin 
                WHERE RoleId = @RoleId 
                  AND Username = @Username 
                  AND PasswordHash = @PasswordHash";

            var parameters = new DynamicParameters();
            parameters.Add("RoleId", loginModel.RoleId);
            parameters.Add("Username", loginModel.Username);
            parameters.Add("PasswordHash", hashedPassword);

            var user = await _dbGateway.ExeScalarQuery<dynamic>(query, parameters);

            if (user == null)
            {
                // Invalid login attempt
                return null;
            }

            // Generate JWT token
            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(dynamic user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("UserId", user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.RoleId.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(30), // Token valid for 30 minutes
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string HashPassword(string password)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] bytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
