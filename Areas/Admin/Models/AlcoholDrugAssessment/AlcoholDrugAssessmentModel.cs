namespace LittleArkFoundation.Areas.Admin.Models.AlcoholDrugAssessment
{
    public class AlcoholDrugAssessmentModel
    {
        public int AlcoholDrugID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public string TobaccoUse { get; set; } = "N/A";
        public string AlcoholUse { get; set; } = "N/A";
        public string RecreationalMedicationUse { get; set; } = "N/A";
        public bool HasOverdosed { get; set; } = false;
        public string OverdoseDate { get; set; } = "N/A";
        public bool HasAlcoholProblems { get; set; } = false;
        public bool LegalProblems { get; set; } = false;
        public bool SocialPeerProblems { get; set; } = false;
        public bool WorkProblems { get; set; } = false;
        public bool FamilyProblems { get; set; } = false;
        public bool FriendsProblems { get; set; } = false;
        public bool FinancialProblems { get; set; } = false;
        public string DescribeProblems { get; set; } = "N/A";
        public bool ContinuedUse { get; set; } = false;
    }
}
