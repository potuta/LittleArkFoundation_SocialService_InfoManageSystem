namespace LittleArkFoundation.Areas.Admin.Models.Referrals
{
    public class ReferralsModel
    {
        public int ReferralID { get; set; }
        public int PatientID { get; set; }
        public string ReferralType { get; set; } = "N/A";
        public string Name { get; set; } = "N/A";
        public string Address { get; set; } = "N/A";
        public string ContactNo { get; set; } = "N/A";
        public DateTime DateOfReferral { get; set; } = DateTime.Now;
    }
}
