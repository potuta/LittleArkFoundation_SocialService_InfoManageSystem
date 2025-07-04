﻿using LittleArkFoundation.Areas.Admin.Models.AlcoholDrugAssessment;
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
        public AssessmentsModel? Assessments { get; set; }
        public ReferralsModel? Referrals { get; set; }
        public InformantsModel? Informants { get; set; }
        public PatientsModel? Patient { get; set; }
        public List<FamilyCompositionModel>? FamilyMembers { get; set; }
        public HouseholdModel? Household { get; set; }
        public MSWDClassificationModel? MSWDClassification { get; set; }
        public MonthlyExpensesModel? MonthlyExpenses { get; set; }
        public UtilitiesModel? Utilities { get; set; }
        public MedicalHistoryModel? MedicalHistory { get; set; }
        public ChildHealthModel? ChildHealth { get; set; }
        public List<DiagnosesModel>? Diagnoses { get; set; }
        public List<MedicationsModel>? Medications { get; set; }
        public List<HospitalizationHistoryModel>? HospitalizationHistory { get; set; }
        public MedicalScreeningsModel? MedicalScreenings {  get; set; }
        public PrimaryCareDoctorModel? PrimaryCareDoctor { get; set; }
        public PresentingProblemsModel? PresentingProblems { get; set; }
        public RecentLossesModel? RecentLosses { get; set; }
        public PregnancyBirthHistoryModel? PregnancyBirthHistory { get; set; }
        public DevelopmentalHistoryModel? DevelopmentalHistory { get; set; }
        public List<MentalHealthHistoryModel>? MentalHealthHistory { get; set; }
        public List<FamilyHistoryModel>? FamilyHistory { get; set; }
        public SafetyConcernsModel? SafetyConcerns { get; set; }
        public CurrentFunctioningModel? CurrentFunctioning { get; set; }
        public ParentChildRelationshipModel? ParentChildRelationship { get; set; }
        public EducationModel? Education { get; set; }
        public EmploymentModel? Employment { get; set; }
        public HousingModel? Housing { get; set; }
        public FosterCareModel? FosterCare { get; set; }
        public AlcoholDrugAssessmentModel? AlcoholDrugAssessment { get; set; }
        public LegalInvolvementModel? LegalInvolvement { get; set; }
        public HistoryOfAbuseModel? HistoryOfAbuse { get; set; }
        public HistoryOfViolenceModel? HistoryOfViolence { get; set; }
        public StrengthsResourcesModel? StrengthsResources { get; set; }
        public GoalsModel? Goals { get; set; }
        public List<ProgressNotesModel>? ProgressNotes { get; set; } 
        public int OpdId { get; set; } = 0;
        public int GeneralAdmissionId { get; set; } = 0;

    }
}
