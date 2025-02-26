namespace LittleArkFoundation.Areas.Admin.Models.Patients
{
    public class PatientsModel
    {
        public int PatientID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public int Age { get; set; }
        public string Sex { get; set; }
        public string ContactNo { get; set; }
        public string PlaceOfBirth { get; set; }    
        public string Gender { get; set; }
        public string Religion { get; set; }
        public string Nationality { get; set; }
        public string PermanentAddress { get; set; }
        public string TemporaryAddress { get; set; }
        public string CivilStatus { get; set; }
        public string EducationLevel { get; set; }
        public string Occupation {  get; set; }
        public decimal MonthlyIncome { get; set; }
        public string PhilhealthPIN { get; set; }
        public string PhilhealthMembership { get; set; }
    }
}
