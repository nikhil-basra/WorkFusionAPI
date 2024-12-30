using Dapper;
using WorkFusionAPI.Interfaces;
using WorkFusionAPI.Models;
using WorkFusionAPI.Utility;

namespace WorkFusionAPI.Services
{
    public class TaskService : ITaskService
    {
        private readonly DBGateway _dbGateway;

        public TaskService(DBGateway dbGateway)
        {
            _dbGateway = dbGateway;
        }

        public async Task<int> CreateTask(TaskModel task)
        {
            var query = @"INSERT INTO tasks (TaskName, Description, AssignedTo,AssignedBy, ProjectId, Priority, Status, StartDate, DueDate, EndDate, CreatedAt, UpdatedAt)
                          VALUES (@TaskName, @Description, @AssignedTo,@AssignedBy, @ProjectId, @Priority, @Status, @StartDate, @DueDate, @EndDate, NOW(), NOW());";
            var parameters = new DynamicParameters(task);
            return await _dbGateway.ExeQuery(query, parameters);
        }

        public async Task<IEnumerable<TaskModel>> GetTaskByManagerId(int managerId)
        {
            var query = @"
        SELECT 
            t.TaskId, 
            t.TaskName, 
            t.Description, 
            t.AssignedTo, 
            t.AssignedBy, 
            t.ProjectId, 
            t.Priority, 
            t.Status, 
            t.StartDate, 
            t.DueDate, 
            t.EndDate, 
            t.CreatedAt, 
            t.UpdatedAt, 
            t.IsActive,
            CONCAT(e.FirstName, ' ', e.LastName) AS EmployeeName,
            p.ProjectName
        FROM 
            tasks t
        JOIN 
            employees e ON t.AssignedTo = e.EmployeeId
        JOIN 
            projects p ON t.ProjectId = p.ProjectId
        WHERE 
            t.AssignedBy = @AssignedBy 
            AND t.IsActive = 1
            AND e.IsActive = 1
            AND p.IsActive = 1;
    ";

            var parameters = new DynamicParameters(new { AssignedBy = managerId });

            return await _dbGateway.QueryAsync<TaskModel>(query, parameters);
        }

        public async Task<IEnumerable<TaskModel>> GetTaskByEmployeeId(int employeeId)
        {
            var query = @"
        SELECT 
            t.TaskId, 
            t.TaskName, 
            t.Description, 
            t.AssignedTo, 
            t.AssignedBy, 
            t.ProjectId, 
            t.Priority, 
            t.Status, 
            t.StartDate, 
            t.DueDate, 
            t.EndDate, 
            t.CreatedAt, 
            t.UpdatedAt, 
            t.IsActive,
            CONCAT(e.FirstName, ' ', e.LastName) AS EmployeeName,
            p.ProjectName
        FROM 
            tasks t
        JOIN 
            employees e ON t.AssignedTo = e.EmployeeId
        JOIN 
            projects p ON t.ProjectId = p.ProjectId
        WHERE 
            t.AssignedTo = @AssignedTo
            AND t.IsActive = 1
            AND e.IsActive = 1
            AND p.IsActive = 1;
    ";

            var parameters = new DynamicParameters(new { AssignedTo = employeeId });

            return await _dbGateway.QueryAsync<TaskModel>(query, parameters);
        }


        public async Task<List<TaskModel>> GetAllTasks()
        {
            var query = "SELECT * FROM tasks WHERE IsActive = 1;";
            return await _dbGateway.ExeQueryList<TaskModel>(query);
        }

        public async Task<TaskModel> GetTaskById(int taskId)
        {
            var query = @"
    SELECT 
        t.TaskId, 
        t.TaskName, 
        t.Description, 
        t.AssignedTo, 
        t.AssignedBy, 
        t.ProjectId, 
        t.Priority, 
        t.Status, 
        t.StartDate, 
        t.DueDate, 
        t.EndDate, 
        t.CreatedAt, 
        t.UpdatedAt, 
        t.IsActive,
        CONCAT(e.FirstName, ' ', e.LastName) AS EmployeeName,
        p.ProjectName
    FROM 
        tasks t
    JOIN 
        employees e ON t.AssignedTo = e.EmployeeId
    JOIN 
        projects p ON t.ProjectId = p.ProjectId
    WHERE 
        t.TaskId = @TaskId 
        AND t.IsActive = 1
        AND e.IsActive = 1
        AND p.IsActive = 1;
    ";

            var parameters = new DynamicParameters(new { TaskId = taskId });

            return await _dbGateway.ExeScalarQuery<TaskModel>(query, parameters);
        }


        public async Task<int> UpdateTask(TaskModel task)
        {
            var query = @"UPDATE tasks SET
                          TaskName = @TaskName,
                          Description = @Description,
                          AssignedTo = @AssignedTo,
                          ProjectId = @ProjectId,
                          Priority = @Priority,
                          Status = @Status,
                          StartDate = @StartDate,
                          DueDate = @DueDate,
                          EndDate = @EndDate,
                          UpdatedAt = NOW(),
                          IsActive = @IsActive
                          WHERE TaskId = @TaskId;";
            var parameters = new DynamicParameters(task);
            return await _dbGateway.ExeQuery(query, parameters);
        }

        public async Task<int> UpdateTaskStatus(TaskStatusModel task)
        {
            var query = @"UPDATE tasks SET
                          Status = @Status,
                          StartDate = @StartDate,
                          EndDate = @EndDate
                          WHERE TaskId = @TaskId;";
            var parameters = new DynamicParameters(task);
            return await _dbGateway.ExeQuery(query, parameters);
        }


        public async Task<int> DeleteTask(int taskId)
        {
            var query = "UPDATE tasks SET IsActive = 0 WHERE TaskId = @TaskId;";
            var parameters = new DynamicParameters(new { TaskId = taskId });
            return await _dbGateway.ExeQuery(query, parameters);
        }



        public async Task<TaskStatusCount> GetTaskCountsAsync(int employeeId)
        {
            var query = @"
            SELECT
                SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END) AS Pending,
                SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) AS Completed,
                SUM(CASE WHEN Status = 'Working On It' THEN 1 ELSE 0 END) AS WorkingOnIt,
                COUNT(*) AS Total
            FROM tasks
            WHERE AssignedTo = @EmployeeId";

            var parameters = new DynamicParameters();
            parameters.Add("EmployeeId", employeeId);

            return await _dbGateway.ExeQuerySingle<TaskStatusCount>(query, parameters);
        }


    }
}
