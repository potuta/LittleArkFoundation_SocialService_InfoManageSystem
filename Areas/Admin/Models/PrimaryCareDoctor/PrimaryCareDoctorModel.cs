namespace LittleArkFoundation.Areas.Admin.Models.PrimaryCareDoctor
{
    public class PrimaryCareDoctorModel
    {
        public int DoctorID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public string? DoctorName { get; set; } = "N/A";
        public string? Facility { get; set; } = "N/A";
        public string? PhoneNumber { get; set; } = "0";
    }
}
