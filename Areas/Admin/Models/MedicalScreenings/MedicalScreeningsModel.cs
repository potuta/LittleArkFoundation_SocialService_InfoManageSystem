namespace LittleArkFoundation.Areas.Admin.Models.MedicalScreenings
{
    public class MedicalScreeningsModel
    {
        public int ScreeningsID { get; set; }
        public int PatientID { get; set; }
        public bool? HasScreeningDone { get; set; }
        public DateOnly? HearingTestDate { get; set; }
        public string? HearingTestOutcome { get; set; }
        public DateOnly? VisionTestDate { get; set; }
        public string? VisionTestOutcome { get; set; }
        public DateOnly? SpeechTestDate { get; set; }
        public string? SpeechTestOutcome { get; set; } 
    }
}
