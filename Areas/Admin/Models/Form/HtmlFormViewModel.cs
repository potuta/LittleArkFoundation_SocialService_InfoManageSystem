namespace LittleArkFoundation.Areas.Admin.Models.Form
{
    public class HtmlFormViewModel
    {
        public int Id { get; set; }
        public int AssessmentID { get; set; }
        public List<string> HtmlPages { get; set; }
        public bool isLatestAssessment { get; set; } = false;
        public bool isActive { get; set; } = true;
    }
}
