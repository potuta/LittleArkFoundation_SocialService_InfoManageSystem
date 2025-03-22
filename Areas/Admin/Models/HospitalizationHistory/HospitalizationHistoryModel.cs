namespace LittleArkFoundation.Areas.Admin.Models.HospitalizationHistory
{
    public class HospitalizationHistoryModel
    {
        public int HospitalizationID { get; set; }
        public int PatientID { get; set; }
        public bool? HasSeriousAccidentOrIllness {  get; set; }
        public string? Reason { get; set; }
        public DateOnly? Date {  get; set; }
        public string? Location { get; set; }
    }
}
