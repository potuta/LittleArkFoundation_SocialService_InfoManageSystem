namespace LittleArkFoundation.Areas.Admin.Models.Medications
{
    public class MedicationsModel
    {
        public int MedicationID { get; set; }
        public int PatientID { get; set; }
        public bool? DoesTakeAnyMedication { get; set; }
        public string? Medication {  get; set; }
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public string? PrescribedBy { get; set; }
        public string? ReasonForMedication { get; set; }
        public bool? IsTakingMedicationAsPrescribed { get; set; }   
        public string? DescribeTakingMedication { get; set; }
        public string? AdditionalInfo { get; set; }
    }
}
