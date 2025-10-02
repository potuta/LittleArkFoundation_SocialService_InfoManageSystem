using LittleArkFoundation.Areas.Admin.Models.Statistics;

namespace LittleArkFoundation.Areas.Admin.Models.OPD
{
    public class OPDViewModel
    {
        public OPDModel OPD { get; set; } = new OPDModel();
        public List<OPDModel> OPDList { get; set; } = new List<OPDModel> { new OPDModel () }; 
        public List<(OPDModel opd, Dictionary<string, int> scores, bool isEligible)> OPDScoringList { get; set; } = new List<(OPDModel opd, Dictionary<string, int> scores, bool isEligible)>();
        public UsersModel User { get; set; } = new UsersModel();
        public List<UsersModel> Users { get; set; } = new List<UsersModel> { new UsersModel() };
        public StatisticsModel? Statistics { get; set; } = new StatisticsModel();
        public List<StatisticsModel> StatisticsList { get; set; } = new List<StatisticsModel> { new StatisticsModel() };
        public OPDPatientsModel OPDPatient { get; set; } = new OPDPatientsModel();
        public List<OPDPatientsModel> OPDPatientsList { get; set; } = new List<OPDPatientsModel> { new OPDPatientsModel() };
        public int OPDId { get; set; } = 0;

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
