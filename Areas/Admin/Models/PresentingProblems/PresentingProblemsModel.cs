namespace LittleArkFoundation.Areas.Admin.Models.PresentingProblems
{
    public class PresentingProblemsModel
    {
        public int ProblemID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public string? PresentingProblem { get; set; } = "N/A";
        public int? Severity { get; set; } = 0;
        public string? ChangeInSleepPattern { get; set; } = "N/A";
        public string? Concentration { get; set; } = "N/A";
        public string? ChangeInAppetite { get; set; } = "N/A";
        public string? IncreasedAnxiety { get; set; } = "N/A";
        public string? MoodSwings { get; set; } = "N/A";
        public string? BehavioralChanges { get; set; } = "N/A";
        public string? Victimization { get; set; } = "N/A";
        public string? DescribeOtherConcern { get; set; } = "N/A";
        public string? DurationOfStress { get; set; } = "N/A";
        public int? CopingLevel { get; set; } = 0;
        public string? OtherFamilySituation { get; set; } = "N/A";
    }
}
