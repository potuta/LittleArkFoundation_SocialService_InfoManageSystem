namespace LittleArkFoundation.Areas.Admin.Models.Household
{
    public class HouseholdModel
    {
        public int HouseholdID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public string OtherSourcesOfIncome { get; set; }
        public int HouseholdSize { get; set; }
        public decimal TotalHouseholdIncome { get; set; }
        public decimal PerCapitaIncome { get; set; }
    }
}
