namespace WorkFusionAPI.Models
{
    public class TaskModel
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public string Description { get; set; }
        public int AssignedTo { get; set; }
        public int AssignedBy { get; set; }
        public int ProjectId { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }

        // Additional fields
        public string? EmployeeName { get; set; }
        public string? ProjectName { get; set; }
    }

    public class TaskStatusModel
    {
        public int TaskId { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

    }

    public class TaskStatusCount
    {
        public int Pending { get; set; }
        public int Completed { get; set; }
        public int WorkingOnIt { get; set; }
        public int Total { get; set; }
    }

}
