namespace LittleArkFoundation.Areas.Admin.Models.LegalInvolvement
{
    public class LegalInvolvementModel
    {
        public int LegalID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public bool HasCustodyCase { get; set; } = false;
        public string DescribeCustodyCase { get; set; } = "N/A";
        public string HasCPSInvolvement { get; set; } = "N/A";
        public string DescribeCPSInvolvement { get; set; } = "N/A";
        public string LegalStatus { get; set; } = "N/A";
        public string ProbationParoleLength { get; set; } = "N/A";
        public string Charges { get; set; } = "N/A";
        public string OfficerName { get; set; } = "N/A";
        public string OfficerContactNum { get; set; } = "N/A";
        public string AdditionalInfo { get; set; } = "N/A";
    }
}
