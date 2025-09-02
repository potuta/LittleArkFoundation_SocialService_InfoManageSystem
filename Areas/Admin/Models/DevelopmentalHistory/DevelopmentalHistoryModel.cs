namespace LittleArkFoundation.Areas.Admin.Models.DevelopmentalHistory
{
    public class DevelopmentalHistoryModel
    {
        public int DevelopmentalHistoryID { get; set; }
        public int AssessmentID { get; set; }   
        public int PatientID { get; set; }
        public decimal RolledOverAge { get; set; } = 0;
        public decimal CrawledAge { get; set; } = 0;
        public decimal WalkedAge { get; set; } = 0;
        public decimal TalkedAge { get; set; } = 0;
        public decimal ToiletTrainedAge { get; set; } = 0;
        public bool SpeechConcerns { get; set; } = false;
        public bool MotorSkillsConcerns { get; set; } = false;
        public bool CognitiveConcerns { get; set; } = false;
        public bool SensoryConcerns { get; set; } = false;
        public bool BehavioralConcerns { get; set; } = false;
        public bool EmotionalConcerns { get; set; } = false;
        public bool SocialConcerns { get; set; } = false;
        public bool HasSignificantDisturbance { get; set; } = false;
        public string DescribeSignificantDisturbance { get; set; } = "N/A";
    }
}
