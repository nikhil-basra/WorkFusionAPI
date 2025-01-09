using System.Collections.Generic;
using System.Threading.Tasks;
using WorkFusionAPI.Models;

namespace WorkFusionAPI.Interfaces
{
    public interface ILeaveService
    {
        Task<IEnumerable<LeaveModel>> GetLeaveRequestsByManagerAsync(int managerId);
        Task<IEnumerable<LeaveModel>> GetPendingLeaveRequestsByManagerAsync(int managerId);
        Task<IEnumerable<LeaveModel>> GetRejectedLeaveRequestsByManagerAsync(int managerId);
        Task<IEnumerable<LeaveModel>> GetApprovedLeaveRequestsByManagerAsync(int managerId);

        Task<bool> SubmitLeaveRequestAsync(LeaveModel leaveRequest);
        Task<bool> AcceptLeaveRequestAsync(int leaveId, int? managerId);
        Task<bool> RejectLeaveRequestAsync(int leaveId, int? managerId);

        Task<IEnumerable<LeaveModel>> GetLeavesByEmployeeIdAsync(int employeeId);

    }

}