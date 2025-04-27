namespace LittleArkFoundation.Areas.Admin.Models.Goals
{
    public class GoalsModel
    {
        public int GoalID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public string CurrentNeeds { get; set; } = "N/A";
        public string HopeToGain { get; set; } = "N/A";
        public string Goal1 { get; set; } = "N/A";
        public string Goal2 { get; set; } = "N/A";
        public string Goal3 { get; set; } = "N/A";
        public string AdditionalInfo { get; set; } = "N/A";
    }
}
