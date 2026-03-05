namespace LittleArkFoundation.Areas.Admin.Models.AgeModifier
{
    public class AgeModifierViewModel
    {
        public AgeModifierModel AgeModifier { get; set; } = new AgeModifierModel();
        public List<AgeModifierModel> AgeModifierList { get; set; } = new List<AgeModifierModel> { new AgeModifierModel() };
        public List<(string age, int modifier)> AgesList { get; set; } = new List<(string, int)>();
    }
}
