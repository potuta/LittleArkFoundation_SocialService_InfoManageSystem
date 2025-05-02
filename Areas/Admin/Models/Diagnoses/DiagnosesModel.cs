namespace LittleArkFoundation.Areas.Admin.Models.Diagnoses
{
    public class DiagnosesModel
    {
        public int DiagnosisID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public string? MedicalCondition { get; set; }
        public bool? ReceivingTreatment { get; set; }
        public string? TreatmentProvider { get; set; }
        public bool? DoesCauseStressOrImpairment { get; set; }
        public string? TreatmentHelp { get; set; }
    }
}
