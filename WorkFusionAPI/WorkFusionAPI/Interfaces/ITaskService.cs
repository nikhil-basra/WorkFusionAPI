using System.Collections.Generic;
using System.Threading.Tasks;
using WorkFusionAPI.Models;


namespace WorkFusionAPI.Interfaces
{
    public interface ITaskService
    {
        Task<int> CreateTask(TaskModel task);
        Task<IEnumerable<TaskModel>> GetTaskByManagerId(int managerId);
        Task<IEnumerable<TaskModel>> GetTaskByEmployeeId(int employeeId);
        Task<List<TaskModel>> GetAllTasks();
        Task<TaskModel> GetTaskById(int taskId);
        Task<int> UpdateTask(TaskModel task);
        Task<int> UpdateTaskStatus(TaskStatusModel task);
        Task<int> DeleteTask(int taskId);


        Task<TaskStatusCount> GetTaskCountsAsync(int employeeId);
    }
}
