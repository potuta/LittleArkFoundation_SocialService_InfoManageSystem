namespace LittleArkFoundation.Areas.Admin.Models.FlagEvaluation
{
    public class FlagEvaluationViewModel
    {
        public FlagEvaluationModel FlagEvaluation { get; set; } = new FlagEvaluationModel();
        public List<FlagEvaluationModel> FlagEvaluationList { get; set; } = new List<FlagEvaluationModel>() { new FlagEvaluationModel() };
    }
}
