namespace LittleArkFoundation.Areas.Admin.Models.HospitalizationHistory
{
    public class HospitalizationHistoryModel
    {
        public int HospitalizationID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public bool? HasSeriousAccidentOrIllness {  get; set; } = false;
        public string? Reason { get; set; } = "N/A";
        public DateOnly? Date {  get; set; } = DateOnly.MinValue;
        public string? Location { get; set; } = "N/A";
    }
}
