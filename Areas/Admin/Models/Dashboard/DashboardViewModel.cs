using LittleArkFoundation.Areas.Admin.Models.Assessments;
using LittleArkFoundation.Areas.Admin.Models.Discharges;
using LittleArkFoundation.Areas.Admin.Models.OPD;

namespace LittleArkFoundation.Areas.Admin.Models.Dashboard
{
    public class DashboardViewModel
    {
        public List<OPDModel> DailyOPD { get; set; } = new List<OPDModel>();
        public List<OPDModel> MonthlyOPD { get; set; } = new List<OPDModel>();
        public List<OPDModel> YearlyOPD { get; set; } = new List<OPDModel>();
        public List<AssessmentsModel> DailyAssessments { get; set; } = new List<AssessmentsModel>();
        public List<AssessmentsModel> MonthlyAssessments { get; set; } = new List<AssessmentsModel>();
        public List<AssessmentsModel> YearlyAssessments { get; set; } = new List<AssessmentsModel>();
        public List<DischargesModel> DailyDischarges { get; set; } = new List<DischargesModel>();
        public List<DischargesModel> MonthlyDischarges { get; set; } = new List<DischargesModel>();
        public List<DischargesModel> YearlyDischarges { get; set; } = new List<DischargesModel>();
    }
}
