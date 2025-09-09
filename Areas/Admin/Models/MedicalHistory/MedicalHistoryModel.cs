namespace LittleArkFoundation.Areas.Admin.Models.MedicalHistory
{
    public class MedicalHistoryModel
    {
        public int HistoryID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public string? AdmittingDiagnosis { get; set; } = "N/A";
        public string? FinalDiagnosis { get; set; } = "N/A";
        public string? DurationSymptomsPriorAdmission { get; set; } = "N/A";
        public string? PreviousTreatmentDuration {  get; set; } = "N/A";
        public string? TreatmentPlan {  get; set; } = "N/A";
        public string? HealthAccessibilityProblems {  get; set; } = "N/A";
    }
}
