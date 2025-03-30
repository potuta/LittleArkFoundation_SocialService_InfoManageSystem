namespace LittleArkFoundation.Areas.Admin.Models.SafetyConcerns
{
    public class SafetyConcernsModel
    {
        public int SafetyConcernID { get; set; }
        public int PatientID { get; set; }
        public bool HasSelfHarm { get; set; } = false;
        public bool IsHomicidal { get; set; } = false;
        public string DescribeHomicidal { get; set; } = "N/A";
        public bool IsSuicidal { get; set; } = false;
        public string DescribeSuicidal { get; set; } = "N/A";
    }
}
