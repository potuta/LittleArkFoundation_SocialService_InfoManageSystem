namespace LittleArkFoundation.Areas.Admin.Models.Referrals
{
    public class ReferralsModel
    {
        public int ReferralID { get; set; }
        public int PatientID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string ContactNo { get; set; }
        public DateTime DateOfReferral { get; set; }
    }
}
