namespace LittleArkFoundation.Areas.Admin.Models.ChildHealth
{
    public class ChildHealthModel
    {
        public int ChildHealthID {  get; set; }
        public int PatientID { get; set; }
        public string? OverallHealth { get; set; }
        public bool? HasHealthIssues { get; set; }
        public string? DescribeHealthIssues { get; set; }
        public bool? HasRecurrentConditions { get; set; }
        public string? DescribeRecurrentConditions { get; set; }
        public bool? HasEarTubes { get; set; }
    }
}
