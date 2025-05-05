namespace LittleArkFoundation.Areas.Admin.Models.Discharges
{
    public class DischargeViewModel
    {
        public List<DischargesModel> Discharges { get; set; } = new List<DischargesModel> { new DischargesModel() };
        public DischargesModel Discharge { get; set; } = new DischargesModel();
        public List<UsersModel> Users { get; set; } = new List<UsersModel> { new UsersModel() };
    }
}
