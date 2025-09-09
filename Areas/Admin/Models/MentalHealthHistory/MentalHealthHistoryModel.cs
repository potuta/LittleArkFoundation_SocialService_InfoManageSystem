namespace LittleArkFoundation.Areas.Admin.Models.MentalHealthHistory
{
    public class MentalHealthHistoryModel
    {
        public int MentalHealthID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public bool HasReceivedCounseling { get; set; } = false;
        public DateOnly DateOfService { get; set; } = DateOnly.MinValue;
        public string Provider { get; set; } = "N/A";
        public string ReasonForTreatment { get; set; } = "N/A";
        public bool WereServicesHelpful { get; set; } = false;
    }
}
