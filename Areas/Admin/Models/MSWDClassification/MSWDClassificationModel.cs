namespace LittleArkFoundation.Areas.Admin.Models.MSWDClassification
{
    public class MSWDClassificationModel
    {
        public int ClassificationID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public string MainClassification { get; set; } = "N/A";
        public string SubClassification { get; set; } = "N/A";
        public string MembershipSector { get; set; } = "N/A";
    }
}
