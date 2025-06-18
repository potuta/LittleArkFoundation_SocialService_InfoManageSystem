namespace LittleArkFoundation.Areas.Admin.Models.OPD
{
    public class OPDViewModel
    {
        public OPDModel OPD { get; set; } = new OPDModel();
        public List<OPDModel> OPDList { get; set; } = new List<OPDModel> { new OPDModel () }; 
        public List<(OPDModel opd, Dictionary<string, int> scores, bool isEligible)> OPDScoringList { get; set; } = new List<(OPDModel opd, Dictionary<string, int> scores, bool isEligible)>();
        public UsersModel User { get; set; } = new UsersModel();
        public List<UsersModel> Users { get; set; } = new List<UsersModel> { new UsersModel() };
    }
}
