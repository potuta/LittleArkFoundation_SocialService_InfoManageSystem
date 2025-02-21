namespace LittleArkFoundation.Areas.Admin.Models
{
    public class BloodInventoryModel
    {
        public int InventoryID { get; set; }
        public string BloodType { get; set; }
        public int Quantity { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string StorageLocation { get; set; }
        public DateTime ReceivedDate { get; set; }
        public string Status { get; set; }
    }   
}