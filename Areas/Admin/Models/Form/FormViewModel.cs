using LittleArkFoundation.Areas.Admin.Models.Assessments;
using LittleArkFoundation.Areas.Admin.Models.ChildHealth;
using LittleArkFoundation.Areas.Admin.Models.Diagnoses;
using LittleArkFoundation.Areas.Admin.Models.FamilyComposition;
using LittleArkFoundation.Areas.Admin.Models.HospitalizationHistory;
using LittleArkFoundation.Areas.Admin.Models.Household;
using LittleArkFoundation.Areas.Admin.Models.Informants;
using LittleArkFoundation.Areas.Admin.Models.MedicalHistory;
using LittleArkFoundation.Areas.Admin.Models.MedicalScreenings;
using LittleArkFoundation.Areas.Admin.Models.Medications;
using LittleArkFoundation.Areas.Admin.Models.MonthlyExpenses;
using LittleArkFoundation.Areas.Admin.Models.MSWDClassification;
using LittleArkFoundation.Areas.Admin.Models.Patients;
using LittleArkFoundation.Areas.Admin.Models.PresentingProblems;
using LittleArkFoundation.Areas.Admin.Models.PrimaryCareDoctor;
using LittleArkFoundation.Areas.Admin.Models.Referrals;


namespace LittleArkFoundation.Areas.Admin.Models.Form
{
    public class FormViewModel
    {
        public AssessmentsModel? Assessments { get; set; }
        public ReferralsModel? Referrals { get; set; }
        public InformantsModel? Informants { get; set; }
        public PatientsModel? Patient { get; set; }
        public List<FamilyCompositionModel>? FamilyMembers { get; set; }
        public FamilyCompositionModel? FamilyComposition { get; set; }
        public HouseholdModel? Household { get; set; }
        public MSWDClassificationModel? MSWDClassification { get; set; }
        public MonthlyExpensesModel? MonthlyExpenses { get; set; }
        public UtilitiesModel? Utilities { get; set; }
        public MedicalHistoryModel? MedicalHistory { get; set; }
        public ChildHealthModel? ChildHealth { get; set; }
        public DiagnosesModel? Diagnosis { get; set; }
        public List<DiagnosesModel>? Diagnoses { get; set; }
        public MedicationsModel? Medication { get; set; }
        public List<MedicationsModel>? Medications { get; set; }
        public HospitalizationHistoryModel? Hospitalization { get; set; }
        public List<HospitalizationHistoryModel>? HospitalizationHistory { get; set; }
        public MedicalScreeningsModel? MedicalScreenings {  get; set; }
        public PrimaryCareDoctorModel? PrimaryCareDoctor { get; set; }
        public PresentingProblemsModel? PresentingProblems { get; set; }

    }
}
