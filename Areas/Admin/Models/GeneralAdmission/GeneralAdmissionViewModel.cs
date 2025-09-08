using LittleArkFoundation.Areas.Admin.Models.ProgressNotes;
using LittleArkFoundation.Areas.Admin.Models.Statistics;

namespace LittleArkFoundation.Areas.Admin.Models.GeneralAdmission
{
    public class GeneralAdmissionViewModel
    {
        public List<GeneralAdmissionModel> GeneralAdmissions { get; set; } = new List<GeneralAdmissionModel> { new GeneralAdmissionModel() };
        public GeneralAdmissionModel GeneralAdmission { get; set; } = new GeneralAdmissionModel();
        public UsersModel? User { get; set; } = new UsersModel();
        public List<UsersModel> Users { get; set; } = new List<UsersModel> { new UsersModel() };
        public List<ProgressNotesModel> ProgressNotes { get; set; } = new List<ProgressNotesModel> { new ProgressNotesModel() };
        public StatisticsModel? Statistics { get; set; } = new StatisticsModel();
        public List<StatisticsModel> StatisticsList { get; set; } = new List<StatisticsModel> { new StatisticsModel() };

        // Pagination properties
        public int? CurrentPage { get; set; } = 1;
        public int? PageSize { get; set; } = 20; // Show 20 logs per page by default
        public int? TotalCount { get; set; } = 0;
        public int? TotalPages => (int)Math.Ceiling((double)TotalCount.Value / PageSize.Value);

        // Statistics filtering properties
        public Dictionary<int, int>? TotalSourcesMonthly { get; set; } = new Dictionary<int, int>();
        public Dictionary<int, int>? TotalCaseloadMonthly { get; set; } = new Dictionary<int, int>();
        public Dictionary<int, int>? TotalOPDMonthly { get; set; } = new Dictionary<int, int>();
        public Dictionary<int, Dictionary<string, int>>? TotalStatisticsMonthly { get; set; } = new Dictionary<int, Dictionary<string, int>>();
    }
}
