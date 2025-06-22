namespace LittleArkFoundation.Areas.Admin.Models.GeneralAdmission
{
    public class GeneralAdmissionViewModel
    {
        public List<GeneralAdmissionModel> GeneralAdmissions { get; set; } = new List<GeneralAdmissionModel> { new GeneralAdmissionModel() };
        public List<UsersModel> Users { get; set; } = new List<UsersModel> { new UsersModel() };
    }
}
