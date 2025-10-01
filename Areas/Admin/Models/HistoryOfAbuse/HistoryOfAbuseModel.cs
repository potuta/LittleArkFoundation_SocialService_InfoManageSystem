namespace LittleArkFoundation.Areas.Admin.Models.HistoryOfAbuse
{
    public class HistoryOfAbuseModel
    {
        public int AbuseID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public bool HasBeenAbused { get; set; } = false;
        public bool SexualAbuse { get; set; } = false;
        public string SexualAbuseByWhom { get; set; } = "N/A";
        public string SexualAbuseAgeOfChild { get; set; } = "0";
        public bool SexualAbuseReported { get; set; } = false;
        public bool PhysicalAbuse { get; set; } = false;
        public string PhysicalAbuseByWhom { get; set; } = "N/A";
        public string PhysicalAbuseAgeOfChild { get; set; } = "0";
        public bool PhysicalAbuseReported { get; set; } = false;
        public bool EmotionalAbuse { get; set; } = false;
        public string EmotionalAbuseByWhom { get; set; } = "N/A";
        public string EmotionalAbuseAgeOfChild { get; set; } = "0";
        public bool EmotionalAbuseReported { get; set; } = false;
        public bool VerbalAbuse { get; set; } = false;
        public string VerbalAbuseByWhom { get; set; } = "N/A";
        public string VerbalAbuseAgeOfChild { get; set; } = "0";
        public bool VerbalAbuseReported { get; set; } = false;
        public bool AbandonedAbuse { get; set; } = false;
        public string AbandonedAbuseByWhom { get; set; } = "N/A";
        public string AbandonedAbuseAgeOfChild { get; set; } = "0";
        public bool AbandonedAbuseReported { get; set; } = false;
        public bool PsychologicalAbuse { get; set; } = false;
        public string PsychologicalAbuseByWhom { get; set; } = "N/A";
        public string PsychologicalAbuseAgeOfChild { get; set; } = "0";
        public bool PsychologicalAbuseReported { get; set; } = false;
        public bool VictimOfBullying { get; set; } = false;
        public bool SafetyConcerns { get; set; } = false;
        public string DescribeSafetyConcerns { get; set; } = "N/A";
        public string AdditionalInfo { get; set; } = "N/A";
    }
}
