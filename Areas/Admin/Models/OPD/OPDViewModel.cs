namespace LittleArkFoundation.Areas.Admin.Models.OPD
{
    public class OPDViewModel
    {
        public OPDModel OPD { get; set; } = new OPDModel();
        public List<OPDModel> OPDList { get; set; } = new List<OPDModel> { new OPDModel () }; 
        public UsersModel User { get; set; } = new UsersModel();
        public List<UsersModel> Users { get; set; } = new List<UsersModel> { new UsersModel() };
    }
}
