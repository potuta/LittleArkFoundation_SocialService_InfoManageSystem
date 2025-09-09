namespace LittleArkFoundation.Areas.Admin.Models.Diagnoses
{
    public class DiagnosesModel
    {
        public int DiagnosisID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public string? MedicalCondition { get; set; } = "N/A";
        public bool? ReceivingTreatment { get; set; } = false;
        public string? TreatmentProvider { get; set; } = "N/A";
        public bool? DoesCauseStressOrImpairment { get; set; } = false;
        public string? TreatmentHelp { get; set; } = "N/A";
    }
}
