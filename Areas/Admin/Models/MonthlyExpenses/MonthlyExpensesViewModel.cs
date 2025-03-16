namespace LittleArkFoundation.Areas.Admin.Models.MonthlyExpenses
{
    public class MonthlyExpensesViewModel
    {
        public MonthlyExpensesModel? MonthlyExpenses { get; set; }
        public List<MonthlyExpensesModel>? MonthlyExpensesList { get; set; }
        public UtilitiesModel? Utilities { get; set; }
        public List<UtilitiesModel>? UtilitiesList { get; set; }
    }
}
