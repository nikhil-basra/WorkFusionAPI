namespace WorkFusionAPI.Models
{
    public class ManagerModel
    {
        public int ManagerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PresentAddress { get; set; }
        public string PermanentAddress { get; set; }
        public string IDType { get; set; }
        public string IDNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int DepartmentId { get; set; }
        public int UserId { get; set; }
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public string? ManagerImage { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}
