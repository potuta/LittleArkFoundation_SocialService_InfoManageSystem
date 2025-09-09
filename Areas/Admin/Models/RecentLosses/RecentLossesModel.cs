namespace LittleArkFoundation.Areas.Admin.Models.RecentLosses
{
    public class RecentLossesModel
    {
        public int RecentLossesID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public bool FamilyMemberLoss { get; set; } = false;
        public bool FriendLoss { get; set; } = false;
        public bool HealthLoss { get; set; } = false;
        public bool LifestyleLoss { get; set; } = false;
        public bool JobLoss { get; set; } = false;
        public bool IncomeLoss { get; set; } = false;
        public bool HousingLoss { get; set; } = false;
        public bool NoneLoss { get; set; } = false;
        public string Name { get; set; } = "N/A";
        public DateOnly Date { get; set; } = DateOnly.MinValue;
        public string NatureOfLoss { get; set; } = "N/A";
        public string OtherLosses { get; set; } = "N/A";
        public string AdditionalInfo { get; set; } = "N/A";

    }
}
