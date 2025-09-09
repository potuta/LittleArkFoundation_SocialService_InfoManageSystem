namespace LittleArkFoundation.Areas.Admin.Models.MonthlyExpenses
{
    public class MonthlyExpensesModel
    {
        public int ExpenseID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public decimal? HouseAndLot { get; set; } = 0;
        public decimal? FoodAndWater { get; set; } = 0;
        public decimal? Education { get; set; } = 0;
        public decimal? Clothing { get; set; } = 0;
        public decimal? Communication { get; set; } = 0;
        public decimal? HouseHelp { get; set; } = 0;
        public decimal? MedicalExpenses { get; set; } = 0;
        public decimal? Transportation { get; set; } = 0;
        public string? Others { get; set; } = "N/A";
        public decimal? OthersAmount { get; set; } = 0;
        public decimal? Total { get; set; } = 0;
    }
}
