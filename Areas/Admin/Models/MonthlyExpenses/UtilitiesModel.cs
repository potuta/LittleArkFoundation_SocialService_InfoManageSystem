namespace LittleArkFoundation.Areas.Admin.Models.MonthlyExpenses
{
    public class UtilitiesModel
    {
        public int UtilityID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public string? LightSource { get; set; } = "N/A";
        public decimal? LightSourceAmount { get; set; } = 0;
        public string? FuelSource { get; set; } = "N/A";
        public decimal? FuelSourceAmount { get; set; } = 0;
        public string? WaterSource { get; set; } = "N/A";
    }
}
