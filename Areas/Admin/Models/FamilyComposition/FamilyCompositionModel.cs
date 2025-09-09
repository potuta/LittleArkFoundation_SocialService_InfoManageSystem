namespace LittleArkFoundation.Areas.Admin.Models.FamilyComposition
{
    public class FamilyCompositionModel
    {
        public int Id { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public string Name { get; set; }
        public decimal Age { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string CivilStatus { get; set; }
        public string RelationshipToPatient { get; set; }
        public bool LivingWithChild {  get; set; }
        public string EducationalAttainment { get; set; }
        public string Occupation { get; set; }
        public decimal MonthlyIncome { get; set; }
    }
}
