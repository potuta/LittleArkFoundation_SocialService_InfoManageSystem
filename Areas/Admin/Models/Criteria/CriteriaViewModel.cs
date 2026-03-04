namespace LittleArkFoundation.Areas.Admin.Models.Criteria
{
    public class CriteriaViewModel
    {
        public List<CriteriaModel> CriteriaList { get; set; } = new List<CriteriaModel> { new CriteriaModel() };
        public List<CriteriaModel> CriteriaDisplayNamesList { get; set; } = new List<CriteriaModel> { new CriteriaModel() };
        public CriteriaModel Criteria { get; set; } = new CriteriaModel();
    }
}
