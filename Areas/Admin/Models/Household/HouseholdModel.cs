namespace LittleArkFoundation.Areas.Admin.Models.Household
{
    public class HouseholdModel
    {
        public int HouseholdID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public string OtherSourcesOfIncome { get; set; } = "N/A";
        public int HouseholdSize { get; set; } = 0;
        public decimal TotalHouseholdIncome { get; set; } = 0;
        public decimal PerCapitaIncome { get; set; } = 0;
    }
}
