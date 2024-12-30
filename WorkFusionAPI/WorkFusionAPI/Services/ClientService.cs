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
    public class ClientService : IClientService
    {
        private readonly DBGateway _dbGateway;

        public ClientService(DBGateway dbGateway)
        {
            _dbGateway = dbGateway;
        }

        public async Task<IEnumerable<ClientModel>> GetAllClientsAsync()
        {
            var query = "SELECT * FROM clients WHERE IsActive = true";
            return await _dbGateway.ExeQueryList<ClientModel>(query);
        }

        public async Task<ClientModel> GetClientByIdAsync(int clientId)
        {
            var query = "SELECT * FROM clients WHERE ClientId = @ClientId AND IsActive = true";
            var parameters = new DynamicParameters();
            parameters.Add("ClientId", clientId);
            return await _dbGateway.ExeScalarQuery<ClientModel>(query, parameters);
        }

        public async Task<bool> CreateClientAsync(ClientModel newClient)
        {
            newClient.IsActive = true;
            newClient.CreatedAt = DateTime.UtcNow;
            newClient.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(newClient.ClientImage))
            {
                newClient.ClientImage = await ConvertToBase64IfNotAlready(newClient.ClientImage);
            }

            var query = @"
                INSERT INTO clients (FirstName, LastName, Gender, Email, Phone, PresentAddress, PermanentAddress, IDType, IDNumber,
                                     DateOfBirth,UserId, CreatedAt, UpdatedAt, IsActive)
                VALUES (@FirstName, @LastName, @Gender, @Email, @Phone, @PresentAddress, @PermanentAddress, @IDType, @IDNumber,
                        @DateOfBirth,@UserId, @CreatedAt, @UpdatedAt, @IsActive)";

            var parameters = new DynamicParameters(newClient);
            var result = await _dbGateway.ExeQuery(query, parameters);
            return result > 0;
        }

        public async Task<bool> UpdateClientAsync(ClientModel client)
        {
            client.UpdatedAt = DateTime.UtcNow;

            if (string.IsNullOrEmpty(client.ClientImage))
            {
                var existingClient = await GetClientByIdAsync(client.ClientId);
                if (existingClient != null)
                {
                    client.ClientImage = existingClient.ClientImage;
                }
            }
            else
            {
                client.ClientImage = await ConvertToBase64IfNotAlready(client.ClientImage);
            }

            var query = @"

                
                 -- Update User table FullName and Email based on the previous email
        UPDATE users 
        SET FullName = CONCAT(@FirstName, ' ', @LastName), 
            Email = @Email
        WHERE Email = (SELECT Email FROM clients WHERE ClientId = @ClientId);


                UPDATE clients SET 
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
                    ClientImage = @ClientImage,
                    UpdatedAt = @UpdatedAt
                WHERE ClientId = @ClientId AND IsActive = true";

            var parameters = new DynamicParameters(client);
            var result = await _dbGateway.ExeQuery(query, parameters);
            return result > 0;
        }

        public async Task<bool> DeleteClientAsync(int clientId)
        {
            var query = "UPDATE clients SET IsActive = false, UpdatedAt = @UpdatedAt WHERE ClientId = @ClientId";
            var parameters = new DynamicParameters();
            parameters.Add("ClientId", clientId);
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
