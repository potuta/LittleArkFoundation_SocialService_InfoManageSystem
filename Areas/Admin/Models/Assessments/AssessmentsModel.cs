namespace LittleArkFoundation.Areas.Admin.Models.Assessments
{
    public class AssessmentsModel
    {
        public int Id { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public string Age { get; set; }
        public DateOnly DateOfInterview { get; set; }
        public TimeOnly TimeOfInterview { get; set; }
        public string? BasicWard { get; set; } = "N/A";
        public string? NonBasicWard { get; set; } = "N/A";
        public string? HealthRecordNo { get; set; } = "N/A";
        public string? MSWDNo { get; set; } = "N/A";
        public string? AssessmentStatement { get; set; } = "N/A";
        public int? UserID { get; set; } = 0;
        public string? ContactNo { get; set; } = "0";
        public string? Gender { get; set; } = "N/A";
        public string? Religion { get; set; } = "N/A";
        public string? PermanentAddress { get; set; } = "N/A";
        public string? TemporaryAddress { get; set; } = "N/A";
        public string? CivilStatus { get; set; } = "N/A";
        public string? EducationLevel { get; set; } = "N/A";
        public string? Occupation { get; set; } = "N/A";
        public decimal? MonthlyIncome { get; set; } = 0;
        public string? PhilhealthPIN { get; set; } = "N/A";
        public string? PhilhealthMembership { get; set; } = "N/A";
    }
}
