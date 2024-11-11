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

                if (isActive)
                {
                    // Register user in UserLogin table if not already registered
                    var checkUserExistsQuery = "SELECT COUNT(1) FROM UserLogin WHERE UserId = @UserId";
                    bool userExists = await _dbGateway.ExeScalarQuery<int>(checkUserExistsQuery, parameters) > 0;

                    if (!userExists)
                    {
                        var registerUserQuery = @"
                    INSERT INTO UserLogin (RoleId, UserId, Username, PasswordHash) 
                    SELECT RoleId, UserId, Username, PasswordHash 
                    FROM Users 
                    WHERE UserId = @UserId";
                        await _dbGateway.ExeQuery(registerUserQuery, parameters);
                    }
                }
                else
                {
                    // Remove user from UserLogin table if already registered
                    var deleteUserQuery = "DELETE FROM UserLogin WHERE UserId = @UserId";
                    await _dbGateway.ExeQuery(deleteUserQuery, parameters);
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
        AND u.IsActive = 1
        AND NOT EXISTS (SELECT 1 FROM Clients c WHERE c.UserId = u.UserId)
        AND NOT EXISTS (SELECT 1 FROM Managers m WHERE m.UserId = u.UserId)
        AND NOT EXISTS (SELECT 1 FROM Employees e WHERE e.UserId = u.UserId);
    ";

            var parameters = new DynamicParameters();
            parameters.Add("@RoleId", roleId);

            // Execute the query and return the list of users
            return await _dbGateway.ExeQueryList<Users>(query, parameters);
        }

    }
}
