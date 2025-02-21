namespace LittleArkFoundation.Areas.Admin.Models
{
    public class BloodRequestsModel
    {
        public int RequestID { get; set; }
        public int HospitalID { get; set; }
        public string BloodType { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public DateTime RequestDate { get; set; }
    }
}