namespace LittleArkFoundation.Areas.Admin.Models.CurrentFunctioning
{
    public class CurrentFunctioningModel
    {
        public int CurrentFunctioningID { get; set; }
        public int PatientID { get; set; }
        public bool EatingConcerns { get; set; } = false;
        public bool HygieneConcerns { get; set; } = false;
        public bool SleepingConcerns { get; set; } = false;
        public bool ActivitiesConcerns { get; set; } = false;
        public bool SocialRelationshipsConcerns { get; set; } = false;
        public string DescribeConcerns { get; set; } = "N/A";
        public int EnergyLevel { get; set; } = 0;
        public int PhysicalLevel { get; set; } = 0;
        public int AnxiousLevel { get; set; } = 0;
        public int HappyLevel { get; set; } = 0;
        public int CuriousLevel { get; set; } = 0; 
        public int AngryLevel { get; set; } = 0;
        public int IntensityLevel { get; set; } = 0;
        public int PersistenceLevel { get; set; } = 0;
        public int SensitivityLevel { get; set; } = 0;
        public int PerceptivenessLevel { get; set; } = 0;
        public int AdaptabilityLevel { get; set; } = 0;
        public int AttentionSpanLevel { get; set; } = 0;
    }
}
