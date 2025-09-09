namespace LittleArkFoundation.Areas.Admin.Models.Patients
{
    public class PatientsModel
    {
        public int PatientID { get; set; }
        public string PatientType { get; set; } = "N/A";
        public bool IsActive { get; set; } = true;
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Sex { get; set; }
        public string PlaceOfBirth { get; set; }    
        public string Nationality { get; set; }
        
    }
}
