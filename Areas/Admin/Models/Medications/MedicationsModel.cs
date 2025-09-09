namespace LittleArkFoundation.Areas.Admin.Models.Medications
{
    public class MedicationsModel
    {
        public int MedicationID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public bool? DoesTakeAnyMedication { get; set; } = false;
        public string? Medication {  get; set; } = "N/A";
        public string? Dosage { get; set; } = "N/A";
        public string? Frequency { get; set; } = "N/A";
        public string? PrescribedBy { get; set; } = "N/A";
        public string? ReasonForMedication { get; set; } = "N/A";
        public bool? IsTakingMedicationAsPrescribed { get; set; } = false;
        public string? DescribeTakingMedication { get; set; } = "N/A";
        public string? AdditionalInfo { get; set; } = "N/A";
    }
}
