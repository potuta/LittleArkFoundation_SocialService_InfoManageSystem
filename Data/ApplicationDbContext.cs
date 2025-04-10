using Microsoft.EntityFrameworkCore;
using LittleArkFoundation.Areas.Admin.Models;
using LittleArkFoundation.Areas.Admin.Models.Form;
using LittleArkFoundation.Areas.Admin.Models.Patients;
using LittleArkFoundation.Areas.Admin.Models.Assessments;
using LittleArkFoundation.Areas.Admin.Models.Referrals;
using LittleArkFoundation.Areas.Admin.Models.Informants;
using LittleArkFoundation.Areas.Admin.Models.FamilyComposition;
using LittleArkFoundation.Areas.Admin.Models.Household;
using LittleArkFoundation.Areas.Admin.Models.MSWDClassification;
using LittleArkFoundation.Areas.Admin.Models.SystemLogs;
using LittleArkFoundation.Areas.Admin.Models.MonthlyExpenses;
using LittleArkFoundation.Areas.Admin.Models.MedicalHistory;
using LittleArkFoundation.Areas.Admin.Models.ChildHealth;
using LittleArkFoundation.Areas.Admin.Models.Diagnoses;
using LittleArkFoundation.Areas.Admin.Models.Medications;
using LittleArkFoundation.Areas.Admin.Models.HospitalizationHistory;
using LittleArkFoundation.Areas.Admin.Models.MedicalScreenings;
using LittleArkFoundation.Areas.Admin.Models.PrimaryCareDoctor;
using LittleArkFoundation.Areas.Admin.Models.PresentingProblems;
using LittleArkFoundation.Areas.Admin.Models.RecentLosses;
using LittleArkFoundation.Areas.Admin.Models.PregnancyBirthHistory;
using LittleArkFoundation.Areas.Admin.Models.DevelopmentalHistory;
using LittleArkFoundation.Areas.Admin.Models.MentalHealthHistory;
using LittleArkFoundation.Areas.Admin.Models.FamilyHistory;
using LittleArkFoundation.Areas.Admin.Models.SafetyConcerns;
using LittleArkFoundation.Areas.Admin.Models.CurrentFunctioning;
using LittleArkFoundation.Areas.Admin.Models.ParentChildRelationship;

namespace LittleArkFoundation.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly string _connectionString;

        public ApplicationDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }
        }

        public DbSet<LogsModel> Logs { get; set; }
        public DbSet<UsersModel> Users { get; set; } 
        public DbSet<UsersArchivesModel> UsersArchives { get; set; }
        public DbSet<RolesModel> Roles { get; set; }
        public DbSet<RolePermissionsModel> RolePermissions { get; set; }
        public DbSet<PermissionsModel> Permissions { get; set; }
        public DbSet<FormResponsesModel> FormResponses { get; set; }
        public DbSet<PatientsModel> Patients { get; set; }
        public DbSet<AssessmentsModel> Assessments { get; set; } 
        public DbSet<ReferralsModel> Referrals { get; set; }
        public DbSet<InformantsModel> Informants { get; set; }
        public DbSet<FamilyCompositionModel> FamilyComposition { get; set; }
        public DbSet<HouseholdModel> Households { get; set; }
        public DbSet<MSWDClassificationModel> MSWDClassification { get; set; }
        public DbSet<MonthlyExpensesModel> MonthlyExpenses { get; set; }
        public DbSet<UtilitiesModel> Utilities { get; set; }
        public DbSet<MedicalHistoryModel> MedicalHistory { get; set; }
        public DbSet<ChildHealthModel> ChildHealth { get; set; }
        public DbSet<DiagnosesModel> Diagnoses { get; set; }
        public DbSet<MedicationsModel> Medications { get; set; }
        public DbSet<HospitalizationHistoryModel> HospitalizationHistory {  get; set; }
        public DbSet<MedicalScreeningsModel> MedicalScreenings { get; set; }
        public DbSet<PrimaryCareDoctorModel> PrimaryCareDoctor {  get; set; }
        public DbSet<PresentingProblemsModel> PresentingProblems { get; set; }
        public DbSet<RecentLossesModel> RecentLosses { get; set; }
        public DbSet<PregnancyBirthHistoryModel> PregnancyBirthHistory { get; set; }
        public DbSet<DevelopmentalHistoryModel> DevelopmentalHistory { get; set; }
        public DbSet<MentalHealthHistoryModel> MentalHealthHistory { get; set; }
        public DbSet<FamilyHistoryModel> FamilyHistory { get; set; }
        public DbSet<SafetyConcernsModel> SafetyConcerns { get; set; }
        public DbSet<CurrentFunctioningModel> CurrentFunctioning { get; set; } 
        public DbSet<ParentChildRelationshipModel> ParentChildRelationship { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Logs
            modelBuilder.Entity<LogsModel>()
                .ToTable("Logs")
                .HasKey(l => l.Id);

            modelBuilder.Entity<LogsModel>()
                .Property(l => l.Id)
                .ValueGeneratedOnAdd();

            // Users & UsersArchives
            // Define primary key for Users entity
            modelBuilder.Entity<UsersModel>()
                .ToTable("Users")
                .HasKey(u => u.UserID); // Assuming 'Id' is the primary key property

            modelBuilder.Entity<UsersArchivesModel>()
                .ToTable("Users_Archives")
                .HasKey(u => u.UserID);

            // Roles 
            modelBuilder.Entity<RolesModel>()
                .ToTable("Roles")
                .HasKey(r => r.RoleID);

            // RolesPermissions
            modelBuilder.Entity<RolePermissionsModel>()
                .ToTable("RolePermissions")
                .HasKey(rp => rp.Id);

            modelBuilder.Entity<RolePermissionsModel>()
                .Property(rp => rp.Id)
                .ValueGeneratedOnAdd();

            // Permissions
            modelBuilder.Entity<PermissionsModel>()
                .ToTable("Permissions")
                .HasKey(p => p.PermissionID);

            // FormResponses
            modelBuilder.Entity<FormResponsesModel>()
                .ToTable("FormResponses")
                .HasKey(i => i.Id);

            modelBuilder.Entity<FormResponsesModel>()
                .Property(i => i.Id)
                .ValueGeneratedOnAdd();

            // Patients
            modelBuilder.Entity<PatientsModel>()
                .ToTable("Patients")
                .HasKey(p => p.PatientID);

            // Assessments
            modelBuilder.Entity<AssessmentsModel>()
                .ToTable("Assessments")
                .HasKey(a => a.AssessmentID);

            modelBuilder.Entity<AssessmentsModel>()
                .Property(a => a.AssessmentID)
                .ValueGeneratedOnAdd();

            // Referrals
            modelBuilder.Entity<ReferralsModel>()
                .ToTable("Referrals")
                .HasKey(r => r.ReferralID);

            modelBuilder.Entity<ReferralsModel>()
                .Property(r => r.ReferralID)
                .ValueGeneratedOnAdd();

            // Informants
            modelBuilder.Entity<InformantsModel>()
                .ToTable("Informants")
                .HasKey(i => i.InformantID);

            modelBuilder.Entity<InformantsModel>()
                .Property(i => i.InformantID)
                .ValueGeneratedOnAdd();

            // FamilyComposition
            modelBuilder.Entity<FamilyCompositionModel>()
                .ToTable("FamilyComposition")
                .HasKey(i => i.Id);

            modelBuilder.Entity<FamilyCompositionModel>()
                .Property(i => i.Id)
                .ValueGeneratedOnAdd();

            // Households
            modelBuilder.Entity<HouseholdModel>()
                .ToTable("Households")
                .HasKey(h => h.HouseholdID);

            modelBuilder.Entity<HouseholdModel>()
                .Property(h => h.HouseholdID)
                .ValueGeneratedOnAdd();

            // MSWDClassification
            modelBuilder.Entity<MSWDClassificationModel>()
                .ToTable("MSWDClassification")
                .HasKey(m => m.ClassificationID);

            modelBuilder.Entity<MSWDClassificationModel>()
                .Property(m => m.ClassificationID)
                .ValueGeneratedOnAdd();

            // MonthlyExpenses
            modelBuilder.Entity<MonthlyExpensesModel>()
                .ToTable("MonthlyExpenses")
                .HasKey(m => m.ExpenseID);

            modelBuilder.Entity<MonthlyExpensesModel>()
                .Property(m => m.ExpenseID)
                .ValueGeneratedOnAdd();

            // Utilities
            modelBuilder.Entity<UtilitiesModel>()
                .ToTable("Utilities")
                .HasKey(u => u.UtilityID);

            modelBuilder.Entity<UtilitiesModel>()
                .Property(u => u.UtilityID)
                .ValueGeneratedOnAdd();

            // MedicalHistory
            modelBuilder.Entity<MedicalHistoryModel>()
                .ToTable("MedicalHistory")
                .HasKey(m => m.HistoryID);

            modelBuilder.Entity<MedicalHistoryModel>()
                .Property(m => m.HistoryID)
                .ValueGeneratedOnAdd();

            // ChildHealth
            modelBuilder.Entity<ChildHealthModel>()
                .ToTable("ChildHealth")
                .HasKey(c => c.ChildHealthID);

            modelBuilder.Entity<ChildHealthModel>()
                .Property(c => c.ChildHealthID)
                .ValueGeneratedOnAdd();

            // Diagnoses
            modelBuilder.Entity<DiagnosesModel>()
                .ToTable("Diagnoses")
                .HasKey(d => d.DiagnosisID);

            modelBuilder.Entity<DiagnosesModel>()
                .Property (d => d.DiagnosisID)
                .ValueGeneratedOnAdd();

            // Medications
            modelBuilder.Entity<MedicationsModel>()
                .ToTable("Medications")
                .HasKey(m => m.MedicationID);

            modelBuilder.Entity<MedicationsModel>()
                .Property(m => m.MedicationID)
                .ValueGeneratedOnAdd();

            // HospitalizationHistory
            modelBuilder.Entity<HospitalizationHistoryModel>()
                .ToTable("HospitalizationHistory")
                .HasKey(h => h.HospitalizationID);

            modelBuilder.Entity<HospitalizationHistoryModel>()
                .Property(h => h.HospitalizationID)
                .ValueGeneratedOnAdd();

            // MedicalScreenings
            modelBuilder.Entity<MedicalScreeningsModel>()
                .ToTable("MedicalScreenings")
                .HasKey(s => s.ScreeningsID);

            modelBuilder.Entity<MedicalScreeningsModel>()
                .Property(s => s.ScreeningsID)
                .ValueGeneratedOnAdd();

            // PrimaryCareDoctor
            modelBuilder.Entity<PrimaryCareDoctorModel>()
                .ToTable("PrimaryCareDoctor")
                .HasKey(d => d.DoctorID);

            modelBuilder.Entity<PrimaryCareDoctorModel>()
                .Property(d => d.DoctorID)
                .ValueGeneratedOnAdd();

            // PresentingProblems
            modelBuilder.Entity<PresentingProblemsModel>()
                .ToTable("PresentingProblems")
                .HasKey(p => p.ProblemID);

            modelBuilder.Entity<PresentingProblemsModel>()
                .Property(p => p.ProblemID)
                .ValueGeneratedOnAdd();

            // RecentLosses
            modelBuilder.Entity<RecentLossesModel>()
                .ToTable("RecentLosses")
                .HasKey(r => r.RecentLossesID);

            modelBuilder.Entity<RecentLossesModel>()
                .Property(r => r.RecentLossesID)
                .ValueGeneratedOnAdd();

            // PregnancyBirthHistory
            modelBuilder.Entity<PregnancyBirthHistoryModel>()
                .ToTable("PregnancyBirthHistory")
                .HasKey(p => p.BirthID);

            modelBuilder.Entity<PregnancyBirthHistoryModel>()
                .Property(p => p.BirthID)
                .ValueGeneratedOnAdd();

            // DevelopmentalHistory
            modelBuilder.Entity<DevelopmentalHistoryModel>()
                .ToTable("DevelopmentalHistory")
                .HasKey(d => d.DevelopmentalHistoryID);

            modelBuilder.Entity<DevelopmentalHistoryModel>()
                .Property(d => d.DevelopmentalHistoryID)
                .ValueGeneratedOnAdd();

            // MentalHealthHistory
            modelBuilder.Entity<MentalHealthHistoryModel>()
                .ToTable("MentalHealthHistory")
                .HasKey(m => m.MentalHealthID);

            modelBuilder.Entity<MentalHealthHistoryModel>()
                .Property(m => m.MentalHealthID)
                .ValueGeneratedOnAdd();

            // FamilyHistory
            modelBuilder.Entity<FamilyHistoryModel>()
                .ToTable("FamilyHistory")
                .HasKey(f => f.FamilyHistoryID);

            modelBuilder.Entity<FamilyHistoryModel>()
                .Property(f => f.FamilyHistoryID)
                .ValueGeneratedOnAdd();

            // SafetyConcerns
            modelBuilder.Entity<SafetyConcernsModel>()
                .ToTable("SafetyConcerns")
                .HasKey(s => s.SafetyConcernID);

            modelBuilder.Entity<SafetyConcernsModel>()
                .Property(s => s.SafetyConcernID)
                .ValueGeneratedOnAdd();

            // CurrentFunctioning
            modelBuilder.Entity<CurrentFunctioningModel>()
                .ToTable("CurrentFunctioning")
                .HasKey(c => c.CurrentFunctioningID);

            modelBuilder.Entity<CurrentFunctioningModel>()
                .Property(c => c.CurrentFunctioningID)
                .ValueGeneratedOnAdd();

            // ParentChildRelationship
            modelBuilder.Entity<ParentChildRelationshipModel>()
                .ToTable("ParentChildRelationship")
                .HasKey(p => p.ParentChildID);

            modelBuilder.Entity<ParentChildRelationshipModel>()
                .Property(p => p.ParentChildID)
                .ValueGeneratedOnAdd();
        }
    }
}