namespace LittleArkFoundation.Areas.Admin.Models.FlagEvaluation
{
    public class FlagEvaluationModel
    {
        public int Id { get; set; }
        public string Flag { get; set; }
        public string Condition { get; set; }
        public int Threshold { get; set; }
    }
}
