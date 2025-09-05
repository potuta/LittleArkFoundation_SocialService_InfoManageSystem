namespace LittleArkFoundation.Areas.Admin.Models.FosterCare
{
    public class FosterCareModel
    {
        public int FosterCareID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public string HasBeenFosterCared { get; set; } = "N/A";
        public decimal FosterAgeStart { get; set; } = 0;
        public decimal FosterAgeEnd { get; set; } = 0;
        public string Reason { get; set; } = "N/A";
        public string PlacementType { get; set; } = "N/A";
        public string CurrentStatus { get; set; } = "N/A";
        public string OutOfCareReason { get; set; } = "N/A";
    }
}
