namespace WorkFusionAPI.Models
{
    public class EmployeeModel
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PresentAddress { get; set; }
        public string PermanentAddress { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? DepartmentId { get; set; }
        public int UserId { get; set; }
        public DateTime? HireDate { get; set; }
        public decimal CurrentSalary { get; set; }
        public bool IsActive { get; set; }
        public string? EmployeeImage { get; set; } // Base64 image
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
