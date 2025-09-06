namespace LittleArkFoundation.Areas.Admin.Models.MSWDClassification
{
    public class MSWDClassificationModel
    {
        public int ClassificationID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public string MainClassification { get; set; }
        public string SubClassification { get; set; } = "";
        public string MembershipSector { get; set; }
    }
}
