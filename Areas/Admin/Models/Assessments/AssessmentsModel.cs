namespace LittleArkFoundation.Areas.Admin.Models.Assessments
{
    public class AssessmentsModel
    {
        public int Id { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public DateOnly DateOfInterview { get; set; }
        public TimeOnly TimeOfInterview { get; set; }
        public string? BasicWard { get; set; }
        public string? NonBasicWard { get; set; }
        public string? HealthRecordNo { get; set; }
        public string? MSWDNo { get; set; }
        public string? AssessmentStatement { get; set; }
    }
}
