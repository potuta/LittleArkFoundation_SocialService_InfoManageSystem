namespace LittleArkFoundation.Areas.Admin.Models.PregnancyBirthHistory
{
    public class PregnancyBirthHistoryModel
    {
        public int BirthID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public bool HasPregnancyComplications { get; set; } = false;
        public string DescribePregnancyComplications { get; set; } = "N/A";
        public bool IsFullTermBirth { get; set; } = false;
        public bool HasBirthComplications { get; set; } = false;
        public string DescribeBirthComplications { get; set; } = "N/A";
        public bool HasConsumedDrugs { get; set; } = false;
        public decimal BirthWeightLbs { get; set; } = 0;
        public decimal BirthWeightOz { get; set; } = 0;
        public string BirthHealth { get; set; } = "N/A";
        public string LengthOfHospitalStay { get; set; } = "N/A";
        public bool PostpartumDepression { get; set; } = false;
        public bool WasChildAdopted { get; set; } = false;
        public decimal ChildAdoptedAge { get; set; } = 0;
        public string AdoptionType { get; set; } = "N/A";
        public string AdoptionCountry { get; set; } = "N/A";
    }
}
