using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using WorkFusionAPI.Interfaces;
using WorkFusionAPI.Models;
using WorkFusionAPI.Utility;

namespace WorkFusionAPI.Services
{
    public class ProjectService : IProjectService
    {
        private readonly DBGateway _dbGateway;

        public ProjectService(DBGateway dbGateway)
        {
            _dbGateway = dbGateway;
        }

        public async Task<IEnumerable<ProjectModel>> GetProjectsAsync()
        {
            var query = "SELECT * FROM projects";
            return await _dbGateway.ExeQueryList<ProjectModel>(query);
        }

        public async Task<ProjectModel> GetProjectByIdAsync(int projectId)
        {
            var query = "SELECT * FROM projects WHERE ProjectId = @ProjectId";
            var parameters = new DynamicParameters();
            parameters.Add("@ProjectId", projectId);
            return await _dbGateway.ExeScalarQuery<ProjectModel>(query, parameters);
        }

       public async Task<bool> AddProjectAsync(ProjectModel project)
{
    var query = @"
    INSERT INTO projects 
    (ProjectName, Description, StartDate, Deadline, EndDate, Budget, ActualCost, Status, 
     ManagerId, ClientId, Attachments, Milestones, TeamMembers, CreatedAt, UpdatedAt, IsActive)
    VALUES 
    (@ProjectName, @Description, @StartDate, @Deadline, @EndDate, @Budget, @ActualCost, @Status, 
     @ManagerId, @ClientId, @Attachments, @Milestones, @TeamMembers, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, @IsActive)";

    var parameters = new DynamicParameters(project);

  
    
    return await _dbGateway.ExeQuery(query, parameters) > 0;
}


        public async Task<bool> UpdateProjectAsync(ProjectModel project)
        {
            var query = @"
        UPDATE projects
        SET 
            ProjectName = @ProjectName,
            Description = @Description,
            StartDate = @StartDate,
            Deadline = @Deadline,
            EndDate = @EndDate,
            Budget = @Budget,
            ActualCost = @ActualCost,
            Status = @Status,
            ManagerId = @ManagerId,
            ClientId = @ClientId,
            Attachments = @Attachments,
            Milestones = @Milestones,
            TeamMembers = @TeamMembers,
            UpdatedAt = CURRENT_TIMESTAMP,
            IsActive = @IsActive
        WHERE ProjectId = @ProjectId";

            var parameters = new DynamicParameters(project);

         
            return await _dbGateway.ExeQuery(query, parameters) > 0;
        }


        public async Task<bool> DeleteProjectAsync(int projectId)
        {
            var query = "DELETE FROM projects WHERE ProjectId = @ProjectId";
            var parameters = new DynamicParameters();
            parameters.Add("@ProjectId", projectId);
            return await _dbGateway.ExeQuery(query, parameters) > 0;
        }
    }
}
