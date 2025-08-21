namespace LittleArkFoundation.Areas.Admin.Models.GeneralAdmission
{
    public class GeneralAdmissionViewModel
    {
        public List<GeneralAdmissionModel> GeneralAdmissions { get; set; } = new List<GeneralAdmissionModel> { new GeneralAdmissionModel() };
        public GeneralAdmissionModel GeneralAdmission { get; set; } = new GeneralAdmissionModel();
        public List<UsersModel> Users { get; set; } = new List<UsersModel> { new UsersModel() };

        // Pagination properties
        public int? CurrentPage { get; set; } = 1;
        public int? PageSize { get; set; } = 20; // Show 20 logs per page by default
        public int? TotalCount { get; set; } = 0;
        public int? TotalPages => (int)Math.Ceiling((double)TotalCount.Value / PageSize.Value);
    }
}
