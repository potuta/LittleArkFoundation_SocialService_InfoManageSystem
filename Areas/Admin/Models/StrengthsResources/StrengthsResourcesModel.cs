namespace LittleArkFoundation.Areas.Admin.Models.StrengthsResources
{
    public class StrengthsResourcesModel
    {
        public int StrengthID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public string Strengths { get; set; } = "N/A";
        public string Limitations { get; set; } = "N/A";
        public string Resources { get; set; } = "N/A";
        public string Experiences { get; set; } = "N/A";
        public string AlreadyDoing { get; set; } = "N/A";
        public bool ParentsSupport { get; set; } = false;
        public bool PartnerSupport { get; set; } = false;
        public bool SiblingsSupport { get; set; } = false;
        public bool ExtendedFamilySupport { get; set; } = false;
        public bool FriendsSupport { get; set; } = false;
        public bool NeighborsSupport { get; set; } = false;
        public bool SchoolStaffSupport { get; set; } = false;
        public bool ChurchSupport { get; set; } = false;
        public bool PastorSupport { get; set; } = false;
        public bool TherapistSupport { get; set; } = false;
        public bool GroupSupport { get; set; } = false;
        public bool CommunityServiceSupport { get; set; } = false;
        public bool DoctorSupport { get; set; } = false;
        public bool OthersSupport { get; set; } = false;
        public string Others { get; set; } = "N/A";
    }
}
