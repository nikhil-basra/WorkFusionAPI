using Dapper;
using System.Security.Cryptography;
using System.Text;
using WorkFusionAPI.Interfaces;
using WorkFusionAPI.Models;
using WorkFusionAPI.Utility;

namespace WorkFusionAPI.Services
{
    public class UserService : IUserService
    {
        private readonly DBGateway _dbGateway;

        public UserService(DBGateway dbGateway)
        {
            _dbGateway = dbGateway;
        }

        public async Task<int> RegisterUser(UserModel user)
        {
            user.PasswordHash = await EncryptPassword(user.PasswordHash);

            var query = @"
                INSERT INTO Users (Username, FullName, Email, PasswordHash, RoleId) 
                VALUES (@Username, @FullName, @Email, @PasswordHash, @RoleId)";

            var parameters = new DynamicParameters();
            parameters.Add("Username", user.Username);
            parameters.Add("FullName", user.FullName);
            parameters.Add("Email", user.Email);
            parameters.Add("PasswordHash", user.PasswordHash);
            parameters.Add("RoleId", user.RoleId);

            return await _dbGateway.ExeQuery(query, parameters);
        }

        public async Task<string> EncryptPassword(string password)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha512.ComputeHash(passwordBytes);
                return await Task.FromResult(BitConverter.ToString(hashBytes).Replace("-", "").ToLower());
            }
        }




        public async Task<IEnumerable<Users>> GetAllUsersAsync()
        {
            var query = "SELECT * FROM Users";
            return await _dbGateway.ExeQueryList<Users>(query);
        }


        public async Task<bool> UpdateUserIsActiveStatus(int userId, bool isActive)
        {
            // Update the IsActive status in the Users table
            var updateUserQuery = "UPDATE Users SET IsActive = @IsActive WHERE UserId = @UserId";
            var parameters = new DynamicParameters();
            parameters.Add("@IsActive", isActive);
            parameters.Add("@UserId", userId);

            var updateResult = await _dbGateway.ExeQuery(updateUserQuery, parameters);

            if (updateResult > 0)
            {
                // Retrieve the RoleId of the user
                var getRoleIdQuery = "SELECT RoleId FROM Users WHERE UserId = @UserId";
                int roleId = await _dbGateway.ExeScalarQuery<int>(getRoleIdQuery, parameters);

                // Conditionally update IsActive in the respective table based on RoleId
                string specificTableUpdateQuery = roleId switch
                {
                    2 => "UPDATE managers SET IsActive = @IsActive WHERE UserId = @UserId",
                    3 => "UPDATE employees SET IsActive = @IsActive WHERE UserId = @UserId",
                    4 => "UPDATE clients SET IsActive = @IsActive WHERE UserId = @UserId",
                    _ => null
                };

                if (specificTableUpdateQuery != null)
                {
                    await _dbGateway.ExeQuery(specificTableUpdateQuery, parameters);
                }

                return true;
            }

            return false;
        }


        public async Task<IEnumerable<Users>> GetUsersByRoleIdAsync(int roleId)
        {
            // Define the query to get users by role ID and exclude already registered users
            var query = @"
        SELECT * 
        FROM Users u
        WHERE u.RoleId = @RoleId 
        AND NOT EXISTS (SELECT 1 FROM Clients c WHERE c.UserId = u.UserId)
        AND NOT EXISTS (SELECT 1 FROM Managers m WHERE m.UserId = u.UserId)
        AND NOT EXISTS (SELECT 1 FROM Employees e WHERE e.UserId = u.UserId);
    ";

            var parameters = new DynamicParameters();
            parameters.Add("@RoleId", roleId);

            // Execute the query and return the list of users
            return await _dbGateway.ExeQueryList<Users>(query, parameters);
        }




        //----------------reset password-------------------//
        public async Task<bool> VerifyUserCredentials(int userId, string username, string password)
        {
            // Hash the provided password
            string hashedPassword = HashPassword(password);

            // Query to check credentials in the Users table
            string query = @"
    SELECT COUNT(1)
    FROM Users
    WHERE UserId = @UserId AND Username = @Username AND PasswordHash = @PasswordHash";

            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);
            parameters.Add("Username", username);
            parameters.Add("PasswordHash", hashedPassword);

            // Check if the user exists in the Users table
            return await _dbGateway.ExeScalarQuery<int>(query, parameters) > 0;
        }

        public async Task<bool> ResetPassword(int userId, string newPassword)
        {
            // Encrypt the new password
            string hashedPassword = HashPassword(newPassword);

            // Update password in the Users table
            string updateUsersQuery = @"
    UPDATE Users
    SET PasswordHash = @PasswordHash
    WHERE UserId = @UserId";

            var parameters = new DynamicParameters();
            parameters.Add("PasswordHash", hashedPassword);
            parameters.Add("UserId", userId);

            var usersResult = await _dbGateway.ExeQuery(updateUsersQuery, parameters);

            return usersResult > 0;
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
