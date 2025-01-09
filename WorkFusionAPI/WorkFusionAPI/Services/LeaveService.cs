using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using WorkFusionAPI.Interfaces;
using WorkFusionAPI.Models;
using WorkFusionAPI.Utility;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace WorkFusionAPI.Services

{
    public class LeaveService : ILeaveService
    {
        private readonly DBGateway _dbGateway;
        public LeaveService(DBGateway dbGateway)
        {
            _dbGateway = dbGateway;
        }


        public async Task<IEnumerable<LeaveModel>> GetLeaveRequestsByManagerAsync(int managerId)
        {
            var query = @"
    SELECT 
        la.Id, 
        la.EmployeeId, 
        e.FirstName AS EmployeeFirstName, 
        e.LastName AS EmployeeLastName, 
        la.LeaveType, 
        la.Reason, 
        la.StartDate, 
        la.EndDate, 
        la.Status, 
        la.DecisionBy, 
        la.DecisionDate,
        la.DepartmentId
        FROM leaveapprovals la
        JOIN employees e ON la.EmployeeId = e.EmployeeId
        JOIN managers m ON m.ManagerId = @ManagerId
        WHERE la.DecisionBy = @ManagerId
        AND e.DepartmentId = m.DepartmentId
        ORDER BY la.StartDate DESC";

            var parameters = new DynamicParameters();
            parameters.Add("ManagerId", managerId);

            return await _dbGateway.ExeQueryList<LeaveModel>(query, parameters);
        }

        public async Task<IEnumerable<LeaveModel>> GetPendingLeaveRequestsByManagerAsync(int managerId)
        {
            var query = @"
            SELECT 
                la.Id, 
                la.EmployeeId, 
                CONCAT(e.FirstName, ' ', e.LastName) AS EmployeeName,
                la.LeaveType, 
                la.Reason, 
                la.StartDate, 
                la.EndDate, 
                la.Status, 
                la.CreatedAt,
                la.DecisionBy,
                la.DecisionDate,
                la.DepartmentId
            FROM leaveapprovals la
            JOIN employees e ON la.EmployeeId = e.EmployeeId
            JOIN managers m ON la.DepartmentId = m.DepartmentId  -- Match the leave's department with the manager's department
            WHERE la.Status = 'Pending'  -- Only pending leave requests
            AND la.DecisionBy IS NULL  -- Ensure that the request has not been processed yet
            AND m.ManagerId = @ManagerId  -- Filter by the manager's department
            ORDER BY la.StartDate DESC";

            var parameters = new DynamicParameters();
            parameters.Add("ManagerId", managerId);

            return await _dbGateway.ExeQueryList<LeaveModel>(query, parameters);
        }

        public async Task<IEnumerable<LeaveModel>> GetRejectedLeaveRequestsByManagerAsync(int managerId)
        {
            var query = @"
        SELECT 
            la.Id, 
            la.EmployeeId, 
            CONCAT(e.FirstName, ' ', e.LastName) AS EmployeeName,
            la.LeaveType, 
            la.Reason, 
            la.StartDate, 
            la.EndDate, 
            la.Status, 
            la.DecisionBy, 
            la.DecisionDate
            FROM leaveapprovals la
            JOIN employees e ON la.EmployeeId = e.EmployeeId
            JOIN managers m ON la.DecisionBy = m.ManagerId
            WHERE la.Status = 'Rejected'
              AND la.DecisionBy = @ManagerId
              AND e.DepartmentId = m.DepartmentId
            ORDER BY la.DecisionDate DESC";

            var parameters = new DynamicParameters();
            parameters.Add("ManagerId", managerId);
            return await _dbGateway.ExeQueryList<LeaveModel>(query, parameters);
        }

        // Approved leaves  list
        public async Task<IEnumerable<LeaveModel>> GetApprovedLeaveRequestsByManagerAsync(int managerId)
        {
            var query = @"
    SELECT 
        la.Id, 
        la.EmployeeId, 
        CONCAT(e.FirstName, ' ', e.LastName) AS EmployeeName,
        la.LeaveType, 
        la.Reason, 
        la.StartDate, 
        la.EndDate, 
        la.Status, 
        la.CreatedAt,
        la.DecisionBy,
        la.DecisionDate,
        la.DepartmentId,
        la.CreatedAt
       FROM leaveapprovals la
    JOIN employees e ON la.EmployeeId = e.EmployeeId
    JOIN managers m ON la.DecisionBy = m.ManagerId
    WHERE la.Status = 'Approved'
      AND la.DecisionBy = @ManagerId
      AND e.DepartmentId = m.DepartmentId
    ORDER BY la.StartDate DESC";
            var parameters = new DynamicParameters();
            parameters.Add("ManagerId", managerId);
            return await _dbGateway.ExeQueryList<LeaveModel>(query, parameters);
        }



        //Submit Leaverequest
        public async Task<bool> SubmitLeaveRequestAsync(LeaveModel leaveRequest)
        {
            // Step 1: Fetch the DepartmentId for the given EmployeeId
            var departmentQuery = @"
            SELECT DepartmentId
            FROM employees
            WHERE EmployeeId = @EmployeeId";

            var parameters = new DynamicParameters();
            parameters.Add("EmployeeId", leaveRequest.EmployeeId);

            var departmentId = await _dbGateway.ExeScalarQuery<int>(departmentQuery, parameters);

            if (departmentId == 0)
            {
                throw new ArgumentException("Department not found for the given Employee ID.");
            }

            // Step 2: Insert the leave request into the leaveapprovals table
            var insertQuery = @"
            INSERT INTO leaveapprovals 
            (EmployeeId, DepartmentId, LeaveType, Reason, StartDate, EndDate, Status, CreatedAt )
            VALUES (@EmployeeId, @DepartmentId, @LeaveType, @Reason, @StartDate, @EndDate, 'Pending', NOW())";

            var insertParameters = new DynamicParameters();
            insertParameters.Add("EmployeeId", leaveRequest.EmployeeId);
            insertParameters.Add("DepartmentId", departmentId);
            insertParameters.Add("LeaveType", leaveRequest.LeaveType);
            insertParameters.Add("Reason", leaveRequest.Reason);
            insertParameters.Add("StartDate", leaveRequest.StartDate);
            insertParameters.Add("EndDate", leaveRequest.EndDate);

            var rowsAffected = await _dbGateway.ExecuteAsync(insertQuery, insertParameters);
            return rowsAffected > 0;
        }

        //Accept leave request
        public async Task<bool> AcceptLeaveRequestAsync(int leaveId, int? managerId)
        {
            if (!managerId.HasValue)
            {
                throw new ArgumentException("ManagerId cannot be null when accepting the leave request.");
            }

            var query = @"
            UPDATE leaveapprovals
            SET Status = 'Approved', DecisionBy = @ManagerId, DecisionDate = NOW()
            WHERE Id = @LeaveId AND Status = 'Pending'";

            var parameters = new DynamicParameters();
            parameters.Add("LeaveId", leaveId);
            parameters.Add("ManagerId", managerId);
            var rowsAffected = await _dbGateway.ExecuteAsync(query, parameters);
            return rowsAffected > 0;
        }

        //Reject leave request
        public async Task<bool> RejectLeaveRequestAsync(int leaveId, int? managerId)
        {
            if (!managerId.HasValue)
            {
                throw new ArgumentException("ManagerId cannot be null when rejecting the leave request.");
            }

            var query = @"
    UPDATE leaveapprovals
    SET Status = 'Rejected', DecisionBy = @ManagerId, DecisionDate = NOW()
    WHERE Id = @LeaveId AND Status = 'Pending'";

            var parameters = new DynamicParameters();
            parameters.Add("LeaveId", leaveId);
            parameters.Add("ManagerId", managerId);
            var rowsAffected = await _dbGateway.ExecuteAsync(query, parameters);
            return rowsAffected > 0;
        }



        //Get leave requests list for specific employee
        public async Task<IEnumerable<LeaveModel>> GetLeavesByEmployeeIdAsync(int employeeId)
        {
            var query = @"
         SELECT 
            la.Id,
            la.EmployeeId,
            la.LeaveType,
            la.Reason,
            la.StartDate,
            la.EndDate,
            la.Status,
            la.DecisionBy,
            la.DecisionDate,
            la.DepartmentId,
            la.CreatedAt
            FROM leaveapprovals la
            WHERE la.EmployeeId = @EmployeeId
            ORDER BY la.StartDate DESC";

            var parameters = new DynamicParameters();
            parameters.Add("EmployeeId", employeeId);
            return await _dbGateway.ExeQueryList<LeaveModel>(query, parameters);
        }

    }
}