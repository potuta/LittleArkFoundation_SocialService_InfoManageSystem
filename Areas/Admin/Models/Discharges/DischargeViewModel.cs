namespace LittleArkFoundation.Areas.Admin.Models.Discharges
{
    public class DischargeViewModel
    {
        public List<DischargesModel> Discharges { get; set; } = new List<DischargesModel> { new DischargesModel() };
        public DischargesModel Discharge { get; set; } = new DischargesModel();
        public List<UsersModel> Users { get; set; } = new List<UsersModel> { new UsersModel() };

        // Pagination properties
        public int? CurrentPage { get; set; } = 1;
        public int? PageSize { get; set; } = 20; // Show 20 logs per page by default
        public int? TotalCount { get; set; } = 0;
        public int? TotalPages => (int)Math.Ceiling((double)TotalCount.Value / PageSize.Value);
    }
}
