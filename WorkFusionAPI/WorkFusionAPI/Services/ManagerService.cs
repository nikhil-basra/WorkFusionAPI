using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using WorkFusionAPI.Interfaces;
using WorkFusionAPI.Models;
using WorkFusionAPI.Utility;

namespace WorkFusionAPI.Services
{
    public class ManagerService : IManagerService
    {
        private readonly DBGateway _dbGateway;

        public ManagerService(DBGateway dbGateway)
        {
            _dbGateway = dbGateway;
        }

        public async Task<IEnumerable<ManagerModel>> GetAllManagersAsync()
        {
            var query = "SELECT * FROM managers WHERE IsActive = true";
            return await _dbGateway.ExeQueryList<ManagerModel>(query);
        }

        public async Task<ManagerModel> GetManagerByIdAsync(int managerId)
        {
            var query = "SELECT * FROM managers WHERE ManagerId = @ManagerId AND IsActive = true";
            var parameters = new DynamicParameters();
            parameters.Add("ManagerId", managerId);
            return await _dbGateway.ExeScalarQuery<ManagerModel>(query, parameters);
        }

        public async Task<bool> CreateManagerAsync(ManagerModel newManager)
        {
            newManager.IsActive = true;
            newManager.CreatedAt = DateTime.UtcNow;
            newManager.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(newManager.ManagerImage))
            {
                newManager.ManagerImage = await ConvertToBase64IfNotAlready(newManager.ManagerImage);
            }

            var query = @"
                INSERT INTO managers (FirstName, LastName, Gender, Email, Phone, PresentAddress, PermanentAddress, IDType, IDNumber,
                                      DateOfBirth, DepartmentId, UserId, HireDate, Salary, ManagerImage, IsActive, CreatedAt, UpdatedAt)
                VALUES (@FirstName, @LastName, @Gender, @Email, @Phone, @PresentAddress, @PermanentAddress, @IDType, @IDNumber,
                        @DateOfBirth, @DepartmentId, @UserId, @HireDate, @Salary, @ManagerImage, @IsActive, @CreatedAt, @UpdatedAt)";

            var parameters = new DynamicParameters(newManager);
            var result = await _dbGateway.ExeQuery(query, parameters);
            return result > 0;
        }

        public async Task<bool> UpdateManagerAsync(ManagerModel manager)
        {
            manager.UpdatedAt = DateTime.UtcNow;

            // Check if ManagerImage is null, and if so, retrieve the existing image from the database
            if (string.IsNullOrEmpty(manager.ManagerImage))
            {
                var existingManager = await GetManagerByIdAsync(manager.ManagerId); // Method to get current manager data
                if (existingManager != null)
                {
                    manager.ManagerImage = existingManager.ManagerImage; // Retain the existing image if no new image is provided
                }
            }
            else
            {
                // If a new image is provided, convert it to Base64 if it isn't already
                manager.ManagerImage = await ConvertToBase64IfNotAlready(manager.ManagerImage);
            }

            var query = @"

                 -- Update User table FullName and Email based on the previous email
        UPDATE users 
        SET FullName = CONCAT(@FirstName, ' ', @LastName), 
            Email = @Email
        WHERE Email = (SELECT Email FROM managers WHERE ManagerId = @ManagerId);

        UPDATE managers SET 
            FirstName = @FirstName,
            LastName = @LastName,
            Gender = @Gender,
            Email = @Email,
            Phone = @Phone,
            PresentAddress = @PresentAddress,
            PermanentAddress = @PermanentAddress,
            IDType = @IDType,
            IDNumber = @IDNumber,
            DateOfBirth = @DateOfBirth,
            DepartmentId = @DepartmentId,
            HireDate = @HireDate,
            Salary = @Salary,
            ManagerImage = @ManagerImage,
            UpdatedAt = @UpdatedAt
        WHERE ManagerId = @ManagerId AND IsActive = true";

            var parameters = new DynamicParameters(manager);
            var result = await _dbGateway.ExeQuery(query, parameters);
            return result > 0;
        }


        public async Task<bool> DeleteManagerAsync(int managerId)
        {
            var query = "UPDATE managers SET IsActive = false, UpdatedAt = @UpdatedAt WHERE ManagerId = @ManagerId";
            var parameters = new DynamicParameters();
            parameters.Add("ManagerId", managerId);
            parameters.Add("UpdatedAt", DateTime.UtcNow);
            var result = await _dbGateway.ExeQuery(query, parameters);
            return result > 0;
        }

        private async Task<string> ConvertToBase64IfNotAlready(string imagePath)
        {
            try
            {
                if (!imagePath.StartsWith("data:image"))
                {
                    var imageBytes = await File.ReadAllBytesAsync(imagePath);
                    return Convert.ToBase64String(imageBytes);
                }
                return imagePath;
            }
            catch
            {
                return imagePath;
            }
        }
    }
}
