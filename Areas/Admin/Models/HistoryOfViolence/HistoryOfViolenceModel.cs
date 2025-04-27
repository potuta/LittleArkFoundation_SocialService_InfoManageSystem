namespace LittleArkFoundation.Areas.Admin.Models.HistoryOfViolence
{
    public class HistoryOfViolenceModel
    {
        public int ViolenceID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public bool HasBeenAccused { get; set; } = false;
        public bool SexualAbuse { get; set; } = false;
        public string SexualAbuseToWhom { get; set; } = "N/A";
        public int SexualAbuseAgeOfChild { get; set; } = 0;
        public bool SexualAbuseReported { get; set; } = false;
        public bool PhysicalAbuse { get; set; } = false;
        public string PhysicalAbuseToWhom { get; set; } = "N/A";
        public int PhysicalAbuseAgeOfChild { get; set; } = 0;
        public bool PhysicalAbuseReported { get; set; } = false;
        public bool EmotionalAbuse { get; set; } = false;
        public string EmotionalAbuseToWhom { get; set; } = "N/A";
        public int EmotionalAbuseAgeOfChild { get; set; } = 0;
        public bool EmotionalAbuseReported { get; set; } = false;
        public bool VerbalAbuse { get; set; } = false;
        public string VerbalAbuseToWhom { get; set; } = "N/A";
        public int VerbalAbuseAgeOfChild { get; set; } = 0;
        public bool VerbalAbuseReported { get; set; } = false;
        public bool AbandonedAbuse { get; set; } = false;
        public string AbandonedAbuseToWhom { get; set; } = "N/A";
        public int AbandonedAbuseAgeOfChild { get; set; } = 0;
        public bool AbandonedAbuseReported { get; set; } = false;
        public bool Bullying { get; set; } = false;
        public string AdditionalInfo { get; set; } = "N/A";

    }
}
