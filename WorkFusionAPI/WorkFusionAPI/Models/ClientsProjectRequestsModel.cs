namespace WorkFusionAPI.Models
{
    public class ClientsProjectRequestsModel
    {
        public int ProjectRequestID { get; set; }
        public int ClientID { get; set; }
        public string ProjectTitle { get; set; }
        public string ProjectDescription { get; set; }
        public string ProjectType { get; set; }
        public string Objectives { get; set; }
        public string KeyDeliverables { get; set; }
        public decimal Budget { get; set; }
        public DateTime PreferredStartDate { get; set; }
        public DateTime Deadline { get; set; }
        public string TargetAudience { get; set; }
        public string DesignPreferences { get; set; }
        public string FunctionalRequirements { get; set; }
        public string TechnologyPreferences { get; set; }
        public string ChallengesToAddress { get; set; }
        public string CompetitorReferences { get; set; }
        public string Attachments { get; set; }
        public string SpecialInstructions { get; set; }
        public string ManagerNotes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
