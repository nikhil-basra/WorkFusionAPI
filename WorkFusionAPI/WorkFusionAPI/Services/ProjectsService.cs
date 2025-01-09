using System.Collections.Generic;
using System.Threading.Tasks;
using WorkFusionAPI.Models;
using WorkFusionAPI.Utility;
using Dapper;
using WorkFusionAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;

namespace WorkFusionAPI.Services
{
    public class ProjectsService : IProjectsService
    {
        private readonly DBGateway _dbGateway;
        private readonly INotificationService _notificationService;

        public ProjectsService(DBGateway dbGateway, INotificationService notificationService)
        {
            _dbGateway = dbGateway;
            _notificationService = notificationService;
        }

        // Get all projects
        public async Task<IEnumerable<ProjectsModel>> GetAllProjectsAsync()
        {
            var query = @"
    SELECT 
        p.ProjectId, 
        p.ProjectName, 
        p.Description, 
        p.StartDate, 
        p.EndDate, 
        p.Budget, 
        p.Status, 
        p.Attachments,
        GROUP_CONCAT(CONCAT(e.FirstName, ' ', e.LastName)) AS TeamMemberNames, -- Get employee names
        m.FirstName AS ManagerFirstName,
        m.LastName AS ManagerLastName,
        m.ManagerId,
        c.FirstName AS ClientFirstName,
        c.LastName AS ClientLastName,
        c.ClientId  
    FROM 
        Projects p
    INNER JOIN 
        Managers m ON p.ManagerId = m.ManagerId
    INNER JOIN
        Clients c ON p.ClientId = c.ClientId
    LEFT JOIN 
        employees e ON FIND_IN_SET(e.EmployeeId, p.TeamMembers) > 0 -- Match EmployeeId with TeamMembers
    GROUP BY
        p.ProjectId";

            // Execute the query and map the result to ProjectsModel
            return await _dbGateway.ExeQueryList<ProjectsModel>(query);
        }



        // Get projects by manager ID
        public async Task<IEnumerable<ProjectsModel>> GetProjectsByManagerIdAsync(int managerId)
        {
            var query = @"
        SELECT 
            p.ProjectId, 
            p.ProjectName, 
            p.Description, 
            p.StartDate, 
            p.EndDate, 
            p.Budget, 
            p.Status, 
            p.Attachments,
            p.Milestones ,
            p.TeamMembers,
            GROUP_CONCAT(CONCAT(e.FirstName, ' ', e.LastName)) AS TeamMemberNames, -- Get names from employees
            p.ActualCost,
            p.IsActive ,
            m.FirstName AS ManagerFirstName,
            m.LastName AS ManagerLastName,
            m.ManagerId,
            c.FirstName AS ClientFirstName,
            c.LastName AS ClientLastName,
            c.ClientId  
        FROM 
          Projects p
         LEFT JOIN 
            Managers m ON p.ManagerId = m.ManagerId
        LEFT JOIN
            Clients c ON p.ClientId = c.ClientId
LEFT JOIN 
        employees e ON FIND_IN_SET(e.EmployeeId, p.TeamMembers) > 0 -- Match EmployeeId with TeamMembers
        WHERE 
            p.managerId = @managerId
            GROUP BY
        p.ProjectId";

            var parameters = new DynamicParameters();
            parameters.Add("managerId", managerId);

            // Execute the query and map the result to ProjectsModel
            return await _dbGateway.ExeQueryList<ProjectsModel>(query, parameters);
        }


        //Get project by ID
        // 
        public async Task<ProjectsModel> GetProjectByIdAsync(int projectId)
        {
            var query = @"
        SELECT 
            p.*,
           c.FirstName AS ClientFirstName,
           c.LastName AS ClientLastName,
           c.ClientId  
        FROM 
            Projects p
        INNER JOIN 
            Clients c ON p.ClientId = c.ClientId
        WHERE 
            p.ProjectId = @ProjectId";

            var parameters = new DynamicParameters();
            parameters.Add("ProjectId", projectId);

            return await _dbGateway.ExeScalarQuery<ProjectsModel>(query, parameters);
        }


        public async Task<int> CreateProjectAsync(ProjectsModel project)
        {
            if (project.CreatedAt == DateTime.MinValue)
            {
                project.CreatedAt = DateTime.UtcNow;
            }
            if (project.UpdatedAt == DateTime.MinValue)
            {
                project.UpdatedAt = DateTime.UtcNow; // Set it to the current time if it's not set
            }

            // Insert query for Projects table
            var insertQuery = @"
        INSERT INTO Projects (ProjectName, Description, StartDate, EndDate, Budget, Status, ManagerId, ClientId, CreatedAt, UpdatedAt, Deadline, ActualCost, Attachments, Milestones, TeamMembers, IsActive)
        VALUES (@ProjectName, @Description, @StartDate, @EndDate, @Budget, @Status, @ManagerId, @ClientId, @CreatedAt, @UpdatedAt, @Deadline, @ActualCost, @Attachments, @Milestones, @TeamMembers, @IsActive);
        SELECT LAST_INSERT_ID();";

            var parameters = new DynamicParameters();
            parameters.Add("ProjectName", project.ProjectName);
            parameters.Add("Description", project.Description);
            parameters.Add("StartDate", project.StartDate);
            parameters.Add("EndDate", project.EndDate);
            parameters.Add("Budget", project.Budget);
            parameters.Add("Status", project.Status);
            parameters.Add("ManagerId", project.ManagerId);
            parameters.Add("ClientId", project.ClientId);
            parameters.Add("CreatedAt", project.CreatedAt);
            parameters.Add("UpdatedAt", project.UpdatedAt);
            parameters.Add("Deadline", project.Deadline);
            parameters.Add("ActualCost", project.ActualCost);
            parameters.Add("Attachments", project.Attachments);
            parameters.Add("Milestones", project.Milestones);
            parameters.Add("TeamMembers", project.TeamMembers);
            parameters.Add("IsActive", project.IsActive);

            // Execute the insert query
            var projectId = await _dbGateway.ExeQuery(insertQuery, parameters);

            // Notify Manager or Client about the new project creation
            await _notificationService.AddNotification(new NotificationModel
            {
                EntityId = project.ManagerId, // Assuming Manager gets notified
                RoleId = 2,
                Message = $"A new project '{project.ProjectName}' has been created By Manager Id = '{project.ManagerId}'",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            // Update IsActive in the clientsprojectrequests table
            var updateQuery = @"
        UPDATE clientsprojectrequests
        SET IsActive = 1
        WHERE ProjectTitle = @ProjectName AND ClientID = @ClientId";

            var updateParameters = new DynamicParameters();
            updateParameters.Add("ProjectName", project.ProjectName);
            updateParameters.Add("ClientId", project.ClientId);

            await _dbGateway.ExeQuery(updateQuery, updateParameters);

            return projectId;
        }


        // update project
        public async Task<int> UpdateProjectAsync(ProjectsModel project)
        {
            // Ensure UpdatedAt has a valid value
            if (project.UpdatedAt == DateTime.MinValue)
            {
                project.UpdatedAt = DateTime.UtcNow; // Set it to the current time if it's not set
            }

            var updateQuery = @"
    UPDATE projects
    SET ProjectName = @ProjectName,
        Description = @Description,
        StartDate = @StartDate,
        EndDate = @EndDate,
        Budget = @Budget,
        Status = @Status,
        ManagerId = @ManagerId,
        ClientId = @ClientId,
        UpdatedAt = @UpdatedAt,
        Deadline = @Deadline,
        ActualCost = @ActualCost,
        Attachments = @Attachments,
        Milestones = @Milestones,
        IsActive = @IsActive
        -- Only update TeamMembers if a non-null value is passed
        {0}
    WHERE ProjectId = @ProjectId";

            // Build the query dynamically based on TeamMembers
            string teamMembersUpdate = string.Empty;
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(project.TeamMembers))
            {
                teamMembersUpdate = ", TeamMembers = @TeamMembers";
                parameters.Add("TeamMembers", project.TeamMembers);
            }

            // Add other parameters
            parameters.Add("ProjectId", project.ProjectId);
            parameters.Add("ProjectName", project.ProjectName);
            parameters.Add("Description", project.Description);
            parameters.Add("StartDate", project.StartDate);
            parameters.Add("EndDate", project.EndDate);
            parameters.Add("Budget", project.Budget);
            parameters.Add("Status", project.Status);
            parameters.Add("ManagerId", project.ManagerId);
            parameters.Add("ClientId", project.ClientId);
            parameters.Add("UpdatedAt", project.UpdatedAt);
            parameters.Add("Deadline", project.Deadline);
            parameters.Add("ActualCost", project.ActualCost);
            parameters.Add("Attachments", project.Attachments);
            parameters.Add("Milestones", project.Milestones);
            parameters.Add("IsActive", project.IsActive);

            // Combine the query with the optional TeamMembers update
            string finalQuery = string.Format(updateQuery, teamMembersUpdate);

            // Notify the manager or client about the project update
            await _notificationService.AddNotification(new NotificationModel
            {
                EntityId = project.ManagerId, // Assuming Manager gets notified
                RoleId = 2,
                Message = $"Project '{project.ProjectName}' has been updated by Manager Id = '{project.ManagerId}'.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            return await _dbGateway.ExecuteAsync(finalQuery, parameters);
        }


        // Delete a project
        public async Task<int> DeleteProjectAsync(int projectId)
        {
            var deleteQuery = "DELETE FROM Projects WHERE ProjectId = @ProjectId";
            var parameters = new DynamicParameters();
            parameters.Add("ProjectId", projectId);

            var result = await _dbGateway.ExecuteAsync(deleteQuery, parameters);

            // Notify the manager or client about the project deletion
            await _notificationService.AddNotification(new NotificationModel
            {
                EntityId = projectId, // Assuming Manager or Client is notified about deletion
                RoleId=2,
                Message = $"Project with ID {projectId} 'has been deleted.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            return result;
        }




        //-------------get project by client id ------------------------//

        public async Task<IEnumerable<ProjectsModel>> GetProjectsByClientIdAsync(int clientId)
                    {
                        var query = @"
            SELECT 
                p.ProjectId, 
                p.ProjectName, 
                p.Description, 
                p.StartDate, 
                p.EndDate, 
                p.Budget, 
                p.Status, 
                p.Attachments, 
                p.Milestones, 
                p.TeamMembers, -- Store the IDs as is
                GROUP_CONCAT(CONCAT(e.FirstName, ' ', e.LastName)) AS TeamMemberNames, -- Get names from employees
                p.ActualCost, 
                p.IsActive, 
                p.ManagerId, 
                p.ClientId, 
                p.CreatedAt, 
                p.UpdatedAt, 
                m.FirstName AS ManagerFirstName, 
                m.LastName AS ManagerLastName
            FROM 
                Projects p
            LEFT JOIN 
                Managers m ON p.ManagerId = m.ManagerId
            LEFT JOIN 
                employees e ON FIND_IN_SET(e.EmployeeId, p.TeamMembers) > 0 -- Match EmployeeId with TeamMembers
            WHERE 
                p.ClientId = @clientId
            GROUP BY 
                p.ProjectId";

                        var parameters = new DynamicParameters();
                        parameters.Add("clientId", clientId);

                        return await _dbGateway.ExeQueryList<ProjectsModel>(query, parameters);
                    }



        // Get projects by employee ID (looking for employee ID in the TeamMembers column)
        public async Task<IEnumerable<ProjectsModel>> GetProjectsByEmployeeIdAsync(int employeeId)
        {
            var query = @"
                SELECT 
                    p.ProjectId, 
                    p.ProjectName, 
                    p.Description, 
                    p.StartDate, 
                    p.EndDate, 
                    p.Budget, 
                    p.Status, 
                    p.Attachments, 
                    p.Milestones,
                    p.TeamMembers,
                    GROUP_CONCAT(CONCAT(e.FirstName, ' ', e.LastName)) AS TeamMemberNames, 
                    p.ActualCost, 
                    p.IsActive,
                    m.FirstName AS ManagerFirstName, 
                    m.LastName AS ManagerLastName,
                    c.FirstName AS ClientFirstName, 
                    c.LastName AS ClientLastName
                FROM 
                    Projects p
                LEFT JOIN 
                    Managers m ON p.ManagerId = m.ManagerId
                LEFT JOIN 
                    Clients c ON p.ClientId = c.ClientId
                LEFT JOIN 
                    employees e ON FIND_IN_SET(e.EmployeeId, p.TeamMembers) > 0 -- Match EmployeeId with TeamMembers
                WHERE 
                    FIND_IN_SET(@employeeId, p.TeamMembers) > 0
                GROUP BY 
                    p.ProjectId";

            var parameters = new DynamicParameters();
            parameters.Add("employeeId", employeeId);

            // Execute the query and return the results
            return await _dbGateway.ExeQueryList<ProjectsModel>(query, parameters);
        }



        //-------------------------------for graphs------------------------

        public async Task<ProjectStatusCountsModel> GetProjectStatusCountsByManagerIdAsync(int managerId)
        {
            var query = @"
        SELECT 
            COUNT(*) AS TotalProjects,
            SUM(CASE WHEN p.Status = 'InProgress' THEN 1 ELSE 0 END) AS InProgressProjects,
            SUM(CASE WHEN p.Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedProjects,
            SUM(CASE WHEN p.Status = 'OnHold' THEN 1 ELSE 0 END) AS OnHoldProjects
        FROM Projects p
        WHERE p.ManagerId = @managerId";

            var parameters = new DynamicParameters();
            parameters.Add("managerId", managerId);

            return await _dbGateway.ExeQuerySingle<ProjectStatusCountsModel>(query, parameters);
        }

        public async Task<ProjectStatusCountsModel> GetProjectStatusCountsAsync()
        {
            var query = @"SELECT 
                   COUNT(*) AS TotalProjects,
                   SUM(CASE WHEN Status = 'InProgress' THEN 1 ELSE 0 END) AS InProgressProjects,
                   SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedProjects,
                   SUM(CASE WHEN Status = 'OnHold' THEN 1 ELSE 0 END) AS OnHoldProjects
               FROM projects
               WHERE IsActive = 1;";

            // Pass null for parameters since no parameters are needed in this query
            var result = await _dbGateway.QueryFirstOrDefaultAsync<ProjectStatusCountsModel>(query, null);
            return result ?? new ProjectStatusCountsModel();
        }


        public async Task<List<DepartmentProjectStatusCountsModel>> GetDepartmentProjectStatusCountsAsync()
        {
            var query = @"
            SELECT 
                d.DepartmentName,
                COUNT(p.ProjectId) AS TotalProjects,
                SUM(CASE WHEN p.Status = 'InProgress' THEN 1 ELSE 0 END) AS InProgressProjects,
                SUM(CASE WHEN p.Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedProjects,
                SUM(CASE WHEN p.Status = 'OnHold' THEN 1 ELSE 0 END) AS OnHoldProjects
            FROM projects p
            INNER JOIN managers m ON p.ManagerId = m.ManagerId
            INNER JOIN departments d ON m.DepartmentId = d.DepartmentId
            WHERE p.IsActive = 1
            GROUP BY d.DepartmentName";

            var result = await _dbGateway.QueryAsync<DepartmentProjectStatusCountsModel>(query, null);
            return result.ToList();
        }


    }
}