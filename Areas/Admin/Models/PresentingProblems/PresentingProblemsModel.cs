namespace LittleArkFoundation.Areas.Admin.Models.PresentingProblems
{
    public class PresentingProblemsModel
    {
        public int ProblemID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public string? PresentingProblem { get; set; }
        public int? Severity { get; set; }
        public string? ChangeInSleepPattern { get; set; }
        public string? Concentration { get; set; }
        public string? ChangeInAppetite { get; set; }
        public string? IncreasedAnxiety { get; set; }
        public string? MoodSwings { get; set; }
        public string? BehavioralChanges { get; set; }
        public string? Victimization { get; set; }
        public string? DescribeOtherConcern { get; set; }
        public string? DurationOfStress { get; set; }
        public int? CopingLevel { get; set; }
        public string? OtherFamilySituation { get; set; }
    }
}
