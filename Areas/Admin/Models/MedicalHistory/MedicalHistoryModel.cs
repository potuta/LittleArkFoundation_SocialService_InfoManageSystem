namespace LittleArkFoundation.Areas.Admin.Models.MedicalHistory
{
    public class MedicalHistoryModel
    {
        public int HistoryID { get; set; }
        public int PatientID { get; set; }
        public string? AdmittingDiagnosis { get; set; }
        public string? FinalDiagnosis { get; set; }
        public string? DurationSymptomsPriorAdmission { get; set; }
        public string? PreviousTreatmentDuration {  get; set; }
        public string? TreatmentPlan {  get; set; }
        public string? HealthAccessibilityProblems {  get; set; }
    }
}
