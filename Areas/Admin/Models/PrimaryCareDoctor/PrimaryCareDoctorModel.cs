namespace LittleArkFoundation.Areas.Admin.Models.PrimaryCareDoctor
{
    public class PrimaryCareDoctorModel
    {
        public int DoctorID { get; set; }
        public int PatientID { get; set; }
        public string? DoctorName { get; set; }
        public string? Facility { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
