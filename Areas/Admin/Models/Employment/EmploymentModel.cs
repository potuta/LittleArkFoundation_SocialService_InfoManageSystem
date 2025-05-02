namespace LittleArkFoundation.Areas.Admin.Models.Employment
{
    public class EmploymentModel
    {
        public int EmploymentID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public bool IsCurrentlyEmployed { get; set; } = false;
        public string Location { get; set; } = "N/A";
        public string JobDuration { get; set; } = "N/A";
        public bool IsEnjoyingJob { get; set; } = false;
    }
}
