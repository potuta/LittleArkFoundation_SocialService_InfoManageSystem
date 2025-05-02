namespace LittleArkFoundation.Areas.Admin.Models.ParentChildRelationship
{
    public class ParentChildRelationshipModel
    {
        public int ParentChildID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public string ParentingExperience { get; set; } = "N/A";
        public string Challenges { get; set; } = "N/A";
        public string DisciplineMethods { get; set; } = "N/A";
    }
}
