namespace WorkFusionAPI.Models
{
    public class LeaveModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string LeaveType { get; set; }

        public string Reason { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public int? DecisionBy { get; set; }
        public DateTime? DecisionDate { get; set; }

        public DateTime? CreatedAt { get; set; }
        public int? DepartmentId { get; set; }
        public string? EmployeeName { get; set; }
    }
}