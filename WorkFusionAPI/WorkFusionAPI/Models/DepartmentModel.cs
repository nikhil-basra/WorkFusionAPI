namespace WorkFusionAPI.Models
{
    public class DepartmentModel
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DepartmentEmployeeCountModel
    {
        public string DepartmentName { get; set; }
        public int ActiveEmployeeCount { get; set; }
    }
}
