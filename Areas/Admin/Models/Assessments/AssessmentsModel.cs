namespace LittleArkFoundation.Areas.Admin.Models.Assessments
{
    public class AssessmentsModel
    {
        public int Id { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public decimal Age { get; set; }
        public DateOnly DateOfInterview { get; set; }
        public TimeOnly TimeOfInterview { get; set; }
        public string? BasicWard { get; set; }
        public string? NonBasicWard { get; set; }
        public string? HealthRecordNo { get; set; }
        public string? MSWDNo { get; set; }
        public string? AssessmentStatement { get; set; }
        public int? UserID { get; set; }
        public string? ContactNo { get; set; }
        public string? Gender { get; set; }
        public string? Religion { get; set; }
        public string? PermanentAddress { get; set; }
        public string? TemporaryAddress { get; set; }
        public string? CivilStatus { get; set; }
        public string? EducationLevel { get; set; }
        public string? Occupation { get; set; }
        public decimal? MonthlyIncome { get; set; }
        public string? PhilhealthPIN { get; set; }
        public string? PhilhealthMembership { get; set; }
    }
}
