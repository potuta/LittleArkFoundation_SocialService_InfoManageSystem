using LittleArkFoundation.Areas.Admin.Models.Assessments;
using LittleArkFoundation.Areas.Admin.Models.Discharges;

namespace LittleArkFoundation.Areas.Admin.Models.Dashboard
{
    public class DashboardViewModel
    {
        public List<AssessmentsModel> DailyAssessments { get; set; } = new List<AssessmentsModel>();
        public List<AssessmentsModel> MonthlyAssessments { get; set; } = new List<AssessmentsModel>();
        public List<AssessmentsModel> YearlyAssessments { get; set; } = new List<AssessmentsModel>();
        public List<DischargesModel> DailyDischarges { get; set; } = new List<DischargesModel>();
        public List<DischargesModel> MonthlyDischarges { get; set; } = new List<DischargesModel>();
        public List<DischargesModel> YearlyDischarges { get; set; } = new List<DischargesModel>();
    }
}
