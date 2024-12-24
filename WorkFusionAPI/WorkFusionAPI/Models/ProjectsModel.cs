namespace WorkFusionAPI.Models
{
    public class ProjectsModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Budget { get; set; }
        public string Status { get; set; }
        public int ManagerId { get; set; }
        public int ClientId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? ClientFirstName { get; set; }
        public string? ClientLastName { get; set; }
        public string? ManagerFirstName { get; set; }
        public string? ManagerLastName { get; set; }
        public DateTime Deadline { get; set; }
        public decimal ActualCost { get; set; }
        public string? Attachments { get; set; }
        public string Milestones { get; set; }
        public string TeamMembers { get; set; }
        public string? TeamMemberNames { get; set; }
        public Boolean IsActive { get; set; }

    }

    public class ProjectStatusCountsModel
    {
        public int TotalProjects { get; set; }
        public int InProgressProjects { get; set; }
        public int CompletedProjects { get; set; }
        public int OnHoldProjects { get; set; }
    }

    public class DepartmentProjectStatusCountsModel
    {
        public string DepartmentName { get; set; }
        public int TotalProjects { get; set; }
        public int InProgressProjects { get; set; }
        public int CompletedProjects { get; set; }
        public int OnHoldProjects { get; set; }
    }


}
