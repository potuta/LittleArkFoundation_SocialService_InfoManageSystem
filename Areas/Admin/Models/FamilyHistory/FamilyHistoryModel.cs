namespace LittleArkFoundation.Areas.Admin.Models.FamilyHistory
{
    public class FamilyHistoryModel
    {
        public int FamilyHistoryID { get; set; }
        public int PatientID { get; set; }
        public bool IsSelf { get; set; } = false;
        public bool HasDepression { get; set; } = false;
        public bool HasAnxiety { get; set; } = false;
        public bool HasBipolarDisorder { get; set; } = false;
        public bool HasSchizophrenia { get; set; } = false;
        public bool HasADHD_ADD { get; set; } = false;
        public bool HasTraumaHistory { get; set; } = false;
        public bool HasAbusiveBehavior { get; set; } = false;
        public bool HasAlcoholAbuse { get; set; } = false;
        public bool HasDrugAbuse { get; set; } = false;
        public bool HasIncarceration { get; set; } = false;
        public string AdditionalInfo { get; set; } = "N/A";
    }
}
