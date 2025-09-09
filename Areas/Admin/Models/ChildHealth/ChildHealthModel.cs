namespace LittleArkFoundation.Areas.Admin.Models.ChildHealth
{
    public class ChildHealthModel
    {
        public int ChildHealthID {  get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public string? OverallHealth { get; set; } = "N/A";
        public bool? HasHealthIssues { get; set; } = false;
        public string? DescribeHealthIssues { get; set; } = "N/A";
        public bool? HasRecurrentConditions { get; set; } = false;
        public string? DescribeRecurrentConditions { get; set; } = "N/A";
        public bool? HasEarTubes { get; set; } = false;
    }
}
