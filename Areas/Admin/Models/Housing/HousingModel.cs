namespace LittleArkFoundation.Areas.Admin.Models.Housing
{
    public class HousingModel
    {
        public int HousingID { get; set; }
        public int PatientID { get; set; }
        public bool IsStable { get; set; } = false;
        public string DescribeIfUnstable { get; set; } = "N/A";
        public string HousingType { get; set; } = "N/A";
        public string DurationLivedInHouse { get; set; } = "N/A";
        public string TimesMoved { get; set; } = "N/A";
        public string AdditionalInfo { get; set; } = "N/A";
    }
}
