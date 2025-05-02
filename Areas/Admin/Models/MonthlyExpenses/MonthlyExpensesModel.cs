namespace LittleArkFoundation.Areas.Admin.Models.MonthlyExpenses
{
    public class MonthlyExpensesModel
    {
        public int ExpenseID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public decimal? HouseAndLot { get; set; }
        public decimal? FoodAndWater { get; set; }
        public decimal? Education { get; set; }
        public decimal? Clothing { get; set; }
        public decimal? Communication { get; set; }
        public decimal? HouseHelp { get; set; }
        public decimal? MedicalExpenses { get; set; }
        public decimal? Transportation { get; set; }
        public string? Others { get; set; }
        public decimal? OthersAmount { get; set; }
        public decimal? Total { get; set; }
    }
}
