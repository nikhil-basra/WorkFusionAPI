using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using WorkFusionAPI.Interfaces;
using WorkFusionAPI.Models;
using WorkFusionAPI.Utility;

namespace WorkFusionAPI.Services
{
    public class ClientsProjectRequestsService : IClientsProjectRequestsService
    {
        private readonly DBGateway _dbGateway;

        public ClientsProjectRequestsService(DBGateway dbGateway)
        {
            _dbGateway = dbGateway;
        }

        public async Task<IEnumerable<ClientsProjectRequestsModel>> GetAllProjectsRequestsAsync()
        {
            var query = "SELECT * FROM clientsprojectrequests";
            return await _dbGateway.ExeQueryList<ClientsProjectRequestsModel>(query);
        }

        public async Task<ClientsProjectRequestsModel> GetProjectRequestsByIdAsync(int projectRequestId)
        {
            var query = "SELECT * FROM clientsprojectrequests WHERE ProjectRequestID = @ProjectRequestID";
            var parameters = new DynamicParameters();
            parameters.Add("@ProjectRequestID", projectRequestId);
            return await _dbGateway.ExeScalarQuery<ClientsProjectRequestsModel>(query, parameters);
        }

        public async Task<bool> AddProjectRequestAsync(ClientsProjectRequestsModel projectRequest)
        {
            var query = @"
                INSERT INTO clientsprojectrequests
                (ClientID, ProjectTitle, ProjectDescription, ProjectType, Objectives, KeyDeliverables, 
                 Budget, PreferredStartDate, Deadline, TargetAudience, DesignPreferences, 
                 FunctionalRequirements, TechnologyPreferences, ChallengesToAddress, CompetitorReferences, 
                 Attachments, SpecialInstructions, ManagerNotes, IsActive, CreatedAt, UpdatedAt)
                VALUES
                (@ClientID, @ProjectTitle, @ProjectDescription, @ProjectType, @Objectives, @KeyDeliverables, 
                 @Budget, @PreferredStartDate, @Deadline, @TargetAudience, @DesignPreferences, 
                 @FunctionalRequirements, @TechnologyPreferences, @ChallengesToAddress, @CompetitorReferences, 
                 @Attachments, @SpecialInstructions, @ManagerNotes, @IsActive, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)";
            var parameters = new DynamicParameters(projectRequest);
            return await _dbGateway.ExeQuery(query, parameters) > 0;
        }

        public async Task<bool> UpdateProjectRequestAsync(ClientsProjectRequestsModel projectRequest)
        {
            var query = @"
                UPDATE clientsprojectrequests
                SET 
                    ProjectTitle = @ProjectTitle,
                    ProjectDescription = @ProjectDescription,
                    ProjectType = @ProjectType,
                    Objectives = @Objectives,
                    KeyDeliverables = @KeyDeliverables,
                    Budget = @Budget,
                    PreferredStartDate = @PreferredStartDate,
                    Deadline = @Deadline,
                    TargetAudience = @TargetAudience,
                    DesignPreferences = @DesignPreferences,
                    FunctionalRequirements = @FunctionalRequirements,
                    TechnologyPreferences = @TechnologyPreferences,
                    ChallengesToAddress = @ChallengesToAddress,
                    CompetitorReferences = @CompetitorReferences,
                    Attachments = @Attachments,
                    SpecialInstructions = @SpecialInstructions,
                    ManagerNotes = @ManagerNotes,
                    UpdatedAt = CURRENT_TIMESTAMP
                WHERE ProjectRequestID = @ProjectRequestID";
            var parameters = new DynamicParameters(projectRequest);
            return await _dbGateway.ExeQuery(query, parameters) > 0;
        }

        public async Task<bool> DeleteProjectRequestAsync(int projectRequestId)
        {
            var query = "DELETE FROM clientsprojectrequests WHERE ProjectRequestID = @ProjectRequestID";
            var parameters = new DynamicParameters();
            parameters.Add("@ProjectRequestID", projectRequestId);
            return await _dbGateway.ExeQuery(query, parameters) > 0;
        }

        public async Task<IEnumerable<ClientsProjectRequestsModel>> GetProjectRequestsByManagerAsync(int managerId)
        {
            // Query to fetch project requests by Manager's department
            var query = @"
        SELECT * 
        FROM clientsprojectrequests
        WHERE ProjectType = (
            SELECT DepartmentName
            FROM departments
            WHERE DepartmentId = (
                SELECT DepartmentId
                FROM managers
                WHERE ManagerId = @ManagerId
            )
        )";

            var parameters = new DynamicParameters();
            parameters.Add("@ManagerId", managerId);

            // Assuming _dbGateway.ExeQueryList executes the query and returns the results
            return await _dbGateway.ExeQueryList<ClientsProjectRequestsModel>(query, parameters);
        }
    }
}
