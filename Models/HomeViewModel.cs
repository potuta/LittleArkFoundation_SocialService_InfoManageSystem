using LittleArkFoundation.Areas.Admin.Models;

namespace LittleArkFoundation.Models
{
    public class HomeViewModel
    {
        public List<BloodInventoryModel>? BloodInventory { get; set; }
        public List<BloodRequestsModel>? RecentRequests { get; set; }
    }
}

