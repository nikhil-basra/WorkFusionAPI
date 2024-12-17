using Dapper;
using System.Threading.Tasks;
using WorkFusionAPI.Interfaces;
using WorkFusionAPI.Utility;

namespace WorkFusionAPI.Services
{
    public class ImageService : IImageService
    {
        private readonly DBGateway _dbGateway;

        public ImageService(DBGateway dbGateway)
        {
            _dbGateway = dbGateway;
        }

        public async Task<string> GetImageByUserIdAndRoleIdAsync(int userId, int roleId)
        {
            string query = string.Empty;
            string tableName = string.Empty;
            string imageColumn = string.Empty;

            switch (roleId)
            {
                case 1: // Manager
                    tableName = "md_admins";
                    imageColumn = "AdminImage";
                    break;
                case 2: // Manager
                    tableName = "managers";
                    imageColumn = "ManagerImage";
                    break;
                case 3: // Employee
                    tableName = "employees";
                    imageColumn = "EmployeeImage";
                    break;
                case 4: // Client
                    tableName = "clients";
                    imageColumn = "ClientImage";
                    break;
                default:
                    return null; // Invalid RoleId
            }

            // Construct the SQL query dynamically
            query = $@"SELECT {imageColumn} 
                       FROM {tableName} 
                       WHERE UserId = @UserId AND IsActive = 1";

            // Add parameters
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            // Execute the query and return the result
            return await _dbGateway.ExeScalarQuery<string>(query, parameters);
        }
    }
}
