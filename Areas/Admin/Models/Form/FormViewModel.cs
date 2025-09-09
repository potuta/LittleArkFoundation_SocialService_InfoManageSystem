using LittleArkFoundation.Areas.Admin.Models.AlcoholDrugAssessment;
using LittleArkFoundation.Areas.Admin.Models.Assessments;
using LittleArkFoundation.Areas.Admin.Models.ChildHealth;
using LittleArkFoundation.Areas.Admin.Models.CurrentFunctioning;
using LittleArkFoundation.Areas.Admin.Models.DevelopmentalHistory;
using LittleArkFoundation.Areas.Admin.Models.Diagnoses;
using LittleArkFoundation.Areas.Admin.Models.Education;
using LittleArkFoundation.Areas.Admin.Models.Employment;
using LittleArkFoundation.Areas.Admin.Models.FamilyComposition;
using LittleArkFoundation.Areas.Admin.Models.FamilyHistory;
using LittleArkFoundation.Areas.Admin.Models.FosterCare;
using LittleArkFoundation.Areas.Admin.Models.Goals;
using LittleArkFoundation.Areas.Admin.Models.HistoryOfAbuse;
using LittleArkFoundation.Areas.Admin.Models.HistoryOfViolence;
using LittleArkFoundation.Areas.Admin.Models.HospitalizationHistory;
using LittleArkFoundation.Areas.Admin.Models.Household;
using LittleArkFoundation.Areas.Admin.Models.Housing;
using LittleArkFoundation.Areas.Admin.Models.Informants;
using LittleArkFoundation.Areas.Admin.Models.LegalInvolvement;
using LittleArkFoundation.Areas.Admin.Models.MedicalHistory;
using LittleArkFoundation.Areas.Admin.Models.MedicalScreenings;
using LittleArkFoundation.Areas.Admin.Models.Medications;
using LittleArkFoundation.Areas.Admin.Models.MentalHealthHistory;
using LittleArkFoundation.Areas.Admin.Models.MonthlyExpenses;
using LittleArkFoundation.Areas.Admin.Models.MSWDClassification;
using LittleArkFoundation.Areas.Admin.Models.ParentChildRelationship;
using LittleArkFoundation.Areas.Admin.Models.Patients;
using LittleArkFoundation.Areas.Admin.Models.PregnancyBirthHistory;
using LittleArkFoundation.Areas.Admin.Models.PresentingProblems;
using LittleArkFoundation.Areas.Admin.Models.PrimaryCareDoctor;
using LittleArkFoundation.Areas.Admin.Models.ProgressNotes;
using LittleArkFoundation.Areas.Admin.Models.RecentLosses;
using LittleArkFoundation.Areas.Admin.Models.Referrals;
using LittleArkFoundation.Areas.Admin.Models.SafetyConcerns;
using LittleArkFoundation.Areas.Admin.Models.StrengthsResources;


namespace LittleArkFoundation.Areas.Admin.Models.Form
{
    public class FormViewModel
    {
        public List<UsersModel>? Users { get; set; } 
        public AssessmentsModel? Assessments { get; set; }  = new AssessmentsModel();
        public ReferralsModel? Referrals { get; set; } = new ReferralsModel();
        public InformantsModel? Informants { get; set; } = new InformantsModel();
        public PatientsModel? Patient { get; set; } = new PatientsModel();
        public List<FamilyCompositionModel>? FamilyMembers { get; set; } = new List<FamilyCompositionModel>() { new FamilyCompositionModel() };
        public HouseholdModel? Household { get; set; } = new HouseholdModel();
        public MSWDClassificationModel? MSWDClassification { get; set; } = new MSWDClassificationModel();
        public MonthlyExpensesModel? MonthlyExpenses { get; set; } = new MonthlyExpensesModel();
        public UtilitiesModel? Utilities { get; set; } = new UtilitiesModel();
        public MedicalHistoryModel? MedicalHistory { get; set; } = new MedicalHistoryModel();
        public ChildHealthModel? ChildHealth { get; set; } = new ChildHealthModel();
        public List<DiagnosesModel>? Diagnoses { get; set; } = new List<DiagnosesModel>() { new DiagnosesModel() };
        public List<MedicationsModel>? Medications { get; set; } = new List<MedicationsModel>() { new MedicationsModel() };
        public List<HospitalizationHistoryModel>? HospitalizationHistory { get; set; } = new List<HospitalizationHistoryModel>() { new HospitalizationHistoryModel() };
        public MedicalScreeningsModel? MedicalScreenings {  get; set; } = new MedicalScreeningsModel();
        public PrimaryCareDoctorModel? PrimaryCareDoctor { get; set; } = new PrimaryCareDoctorModel();
        public PresentingProblemsModel? PresentingProblems { get; set; } = new PresentingProblemsModel();
        public RecentLossesModel? RecentLosses { get; set; } = new RecentLossesModel();
        public PregnancyBirthHistoryModel? PregnancyBirthHistory { get; set; } = new PregnancyBirthHistoryModel();
        public DevelopmentalHistoryModel? DevelopmentalHistory { get; set; } = new DevelopmentalHistoryModel();
        public List<MentalHealthHistoryModel>? MentalHealthHistory { get; set; } = new List<MentalHealthHistoryModel>() { new MentalHealthHistoryModel() };
        public List<FamilyHistoryModel>? FamilyHistory { get; set; } = new List<FamilyHistoryModel>() { new FamilyHistoryModel() };
        public SafetyConcernsModel? SafetyConcerns { get; set; } = new SafetyConcernsModel();
        public CurrentFunctioningModel? CurrentFunctioning { get; set; } = new CurrentFunctioningModel();
        public ParentChildRelationshipModel? ParentChildRelationship { get; set; } = new ParentChildRelationshipModel();
        public EducationModel? Education { get; set; } = new EducationModel();
        public EmploymentModel? Employment { get; set; } = new EmploymentModel();
        public HousingModel? Housing { get; set; } = new HousingModel();
        public FosterCareModel? FosterCare { get; set; } = new FosterCareModel();
        public AlcoholDrugAssessmentModel? AlcoholDrugAssessment { get; set; } = new AlcoholDrugAssessmentModel();
        public LegalInvolvementModel? LegalInvolvement { get; set; } = new LegalInvolvementModel();
        public HistoryOfAbuseModel? HistoryOfAbuse { get; set; } = new HistoryOfAbuseModel();
        public HistoryOfViolenceModel? HistoryOfViolence { get; set; } = new HistoryOfViolenceModel();
        public StrengthsResourcesModel? StrengthsResources { get; set; } = new StrengthsResourcesModel();
        public GoalsModel? Goals { get; set; } = new GoalsModel();
        public List<ProgressNotesModel>? ProgressNotes { get; set; } = new List<ProgressNotesModel>() { new ProgressNotesModel() };
        public int OpdId { get; set; } = 0;
        public int GeneralAdmissionId { get; set; } = 0;

    }
}
