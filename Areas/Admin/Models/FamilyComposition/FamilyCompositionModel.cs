namespace LittleArkFoundation.Areas.Admin.Models.FamilyComposition
{
    public class FamilyCompositionModel
    {
        public int Id { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public string Name { get; set; } = "N/A";
        public string Age { get; set; } = "0";
        public DateOnly DateOfBirth { get; set; } = DateOnly.MinValue;
        public string CivilStatus { get; set; } = "N/A";
        public string RelationshipToPatient { get; set; } = "N/A";
        public bool LivingWithChild {  get; set; } = false;
        public string EducationalAttainment { get; set; } = "N/A";
        public string Occupation { get; set; } = "N/A";
        public decimal MonthlyIncome { get; set; } = 0;
    }
}
