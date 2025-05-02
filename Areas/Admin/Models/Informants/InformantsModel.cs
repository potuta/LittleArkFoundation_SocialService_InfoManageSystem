namespace LittleArkFoundation.Areas.Admin.Models.Informants
{
    public class InformantsModel
    {
        public int InformantID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public string Name { get; set; }
        public string RelationToPatient { get; set; }
        public string ContactNo { get; set; }
        public string Address { get; set; }
        public DateTime DateOfInformant { get; set; }
    }
}
