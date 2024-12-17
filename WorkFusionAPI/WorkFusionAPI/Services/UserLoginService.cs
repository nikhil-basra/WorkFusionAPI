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

            // Query to check if RoleId, (Username or Email), PasswordHash match, and IsActive is 1
            string query = @"
                SELECT UserId, Username, RoleId 
                FROM Users
                WHERE RoleId = @RoleId 
                  AND (Username = @UsernameOrEmail OR Email = @UsernameOrEmail)
                  AND PasswordHash = @PasswordHash
                  AND IsActive = 1";

            var parameters = new DynamicParameters();
            parameters.Add("RoleId", loginModel.RoleId);
            parameters.Add("UsernameOrEmail", loginModel.UsernameOrEmail); // Supports both username and email
            parameters.Add("PasswordHash", hashedPassword);

            var user = await _dbGateway.ExeScalarQuery<dynamic>(query, parameters);

            if (user == null)
            {
                // Invalid login attempt or user is inactive
                return null;
            }

            // Fetch additional details based on the RoleId
            string roleQuery = string.Empty;
            if (user.RoleId == 3) // Employee
            {
                roleQuery = @"
                    SELECT EmployeeId AS EntityId, CONCAT(FirstName, ' ', LastName) AS Name
                    FROM employees
                    WHERE UserId = @UserId AND IsActive = 1";
            }
            else if (user.RoleId == 2) // Manager
            {
                roleQuery = @"
                    SELECT ManagerId AS EntityId, CONCAT(FirstName, ' ', LastName) AS Name
                    FROM managers
                    WHERE UserId = @UserId AND IsActive = 1";
            }
            else if (user.RoleId == 4) // Client
            {
                roleQuery = @"
                    SELECT ClientId AS EntityId, CONCAT(FirstName, ' ', LastName) AS Name
                    FROM clients
                    WHERE UserId = @UserId AND IsActive = 1";
            }
            else if (user.RoleId == 1) // admin
            {
                roleQuery = @"
                    SELECT AdminId AS EntityId, CONCAT(FirstName, ' ', LastName) AS Name
                    FROM md_admins
                    WHERE UserId = @UserId AND IsActive = 1";
            }

            if (!string.IsNullOrEmpty(roleQuery))
            {
                var roleParams = new DynamicParameters();
                roleParams.Add("UserId", user.UserId);

                var roleDetails = await _dbGateway.ExeScalarQuery<dynamic>(roleQuery, roleParams);

                if (roleDetails != null)
                {
                    user.EntityId = roleDetails.EntityId;
                    user.Name = roleDetails.Name;
                }
            }

            // Generate JWT token
            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(dynamic user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var claims = new List<Claim>
            {
                new Claim("UserId", user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.RoleId.ToString())
            };

            // Add specific claims for employee, manager, or client
            if (user.EntityId != null)
            {
                claims.Add(new Claim("EntityId", user.EntityId.ToString()));
                claims.Add(new Claim("FullName", user.Name));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
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
