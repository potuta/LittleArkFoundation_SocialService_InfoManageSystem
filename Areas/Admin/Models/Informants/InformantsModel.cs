namespace LittleArkFoundation.Areas.Admin.Models.Informants
{
    public class InformantsModel
    {
        public int InformantID { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public string Name { get; set; } = "N/A";
        public string RelationToPatient { get; set; } = "N/A";
        public string ContactNo { get; set; } = "0";
        public string Address { get; set; } = "N/A";
        public DateTime DateOfInformant { get; set; } = DateTime.MinValue;
    }
}
