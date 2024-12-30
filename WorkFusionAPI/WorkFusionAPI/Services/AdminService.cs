using WorkFusionAPI.Interfaces;
using WorkFusionAPI.Models;
using WorkFusionAPI.Utility;
using Dapper;

namespace WorkFusionAPI.Services
{
    public class AdminService : IAdminService
    {
        private readonly DBGateway _dbGateway;

        public AdminService(DBGateway dbGateway)
        {
            _dbGateway = dbGateway;
        }

        public async Task<AdminModel> GetAdminByUserIdAsync(int userId)
        {
            var query = "SELECT * FROM md_admins WHERE UserId = @UserId AND IsActive = true";
            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);
            return await _dbGateway.ExeScalarQuery<AdminModel>(query, parameters);
        }

  

        public async Task<bool> UpdateAdminByUserIdAsync(int userId, AdminModel admin)
        {
            admin.UpdatedAt = DateTime.Now;

            var query = @"

             -- Update User table FullName and Email based on the previous email
        UPDATE users 
        SET FullName = CONCAT(@FirstName, ' ', @LastName), 
            Email = @Email
        WHERE Email = (SELECT Email FROM md_admins WHERE UserId = @UserId);



            UPDATE md_admins SET 
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
                AdminImage = @AdminImage,
                UpdatedAt = @UpdatedAt
            WHERE UserId = @UserId AND IsActive = true";

            var parameters = new DynamicParameters(admin);
            parameters.Add("UserId", userId);

            var result = await _dbGateway.ExeQuery(query, parameters);

            return result > 0;
        }

    
    }
}
