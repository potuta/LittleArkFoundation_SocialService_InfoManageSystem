namespace LittleArkFoundation.Areas.Admin.Models.MonthlyExpenses
{
    public class UtilitiesModel
    {
        public int UtilityID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public string? LightSource { get; set; }
        public decimal? LightSourceAmount { get; set; }
        public string? FuelSource { get; set; }
        public decimal? FuelSourceAmount { get; set; }
        public string? WaterSource { get; set; }
    }
}
