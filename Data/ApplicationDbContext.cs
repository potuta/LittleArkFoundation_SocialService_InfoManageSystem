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
        }
    }
}