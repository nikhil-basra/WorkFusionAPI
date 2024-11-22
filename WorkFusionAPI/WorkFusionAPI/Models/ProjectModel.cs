namespace WorkFusionAPI.Models
{
    public class ProjectModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? Budget { get; set; }
        public decimal? ActualCost { get; set; }
        public string? Status { get; set; }
        public int? ManagerId { get; set; }
        public int? ClientId { get; set; }
        public string? Attachments { get; set; } // Stores base64 files as JSON
        public string? Milestones { get; set; }
        public string? TeamMembers { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }  // This is now handled as a boolean
    }
}
