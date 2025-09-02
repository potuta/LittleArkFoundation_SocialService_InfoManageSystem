namespace LittleArkFoundation.Areas.Admin.Models.GeneralAdmission
{
    public class GeneralAdmissionModel
    {
        public int Id { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public DateOnly Date { get; set; } = DateOnly.MinValue;
        public bool isOld { get; set; } = false;
        public int HospitalNo { get; set; } = 0;
        public string FirstName { get; set; } = "N/A";
        public string MiddleName { get; set; } = "N/A";
        public string LastName { get; set; } = "N/A";
        public string Ward { get; set; } = "N/A";
        public string Class { get; set; } = "N/A";
        public decimal Age { get; set; } = 0;
        public string Gender { get; set; } = "N/A";
        public TimeOnly Time { get; set; } = TimeOnly.MinValue;
        public string Diagnosis { get; set; } = "N/A";
        public string CompleteAddress { get; set; } = "N/A";
        public string Origin { get; set; } = "N/A";
        public string ContactNumber { get; set; } = "N/A";
        public string Referral { get; set; } = "N/A";
        public string Occupation { get; set; } = "N/A";
        public string StatsOccupation { get; set; } = "N/A";
        public string IncomeRange { get; set; } = "N/A";
        public decimal MonthlyIncome { get; set; } = 0;
        public string EconomicStatus { get; set; } = "N/A";
        public int HouseholdSize { get; set; } = 0;
        public string MaritalStatus { get; set; } = "N/A";
        public bool isPWD { get; set; } = false;
        public string EducationalAttainment { get; set; } = "N/A";
        public string FatherEducationalAttainment { get; set; } = "N/A";
        public string MotherEducationalAttainment { get; set; } = "N/A";
        public bool isInterviewed { get; set; } = false;
        public string DwellingType { get; set; } = "N/A";
        public string LightSource { get; set; } = "N/A";
        public string WaterSource { get; set; } = "N/A";
        public string FuelSource { get; set; } = "N/A";
        public string PHIC { get; set; } = "N/A";
        public string MSW { get; set; } = "N/A";
        public int UserID { get; set; } = 0;

    }
}
