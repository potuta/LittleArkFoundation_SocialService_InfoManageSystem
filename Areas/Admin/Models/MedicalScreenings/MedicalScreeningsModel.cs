namespace LittleArkFoundation.Areas.Admin.Models.MedicalScreenings
{
    public class MedicalScreeningsModel
    {
        public int ScreeningsID { get; set; }
        public int PatientID { get; set; }
        public bool HasScreeningDone { get; set; } = false;
        public DateOnly HearingTestDate { get; set; } = new DateOnly(1900, 1, 1);
        public string HearingTestOutcome { get; set; } = "N/A";
        public DateOnly VisionTestDate { get; set; } = new DateOnly(1900, 1, 1);
        public string VisionTestOutcome { get; set; } = "N/A";
        public DateOnly SpeechTestDate { get; set; } = new DateOnly(1900, 1, 1);
        public string SpeechTestOutcome { get; set; } = "N/A"; 
    }
}
