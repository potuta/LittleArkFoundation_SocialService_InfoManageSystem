using ClosedXML.Excel;
using LittleArkFoundation.Areas.Admin.Data;
using LittleArkFoundation.Areas.Admin.Models;
using LittleArkFoundation.Areas.Admin.Models.AlcoholDrugAssessment;
using LittleArkFoundation.Areas.Admin.Models.Assessments;
using LittleArkFoundation.Areas.Admin.Models.ChildHealth;
using LittleArkFoundation.Areas.Admin.Models.CurrentFunctioning;
using LittleArkFoundation.Areas.Admin.Models.DevelopmentalHistory;
using LittleArkFoundation.Areas.Admin.Models.Diagnoses;
using LittleArkFoundation.Areas.Admin.Models.Discharges;
using LittleArkFoundation.Areas.Admin.Models.Education;
using LittleArkFoundation.Areas.Admin.Models.Employment;
using LittleArkFoundation.Areas.Admin.Models.FamilyComposition;
using LittleArkFoundation.Areas.Admin.Models.FamilyHistory;
using LittleArkFoundation.Areas.Admin.Models.Form;
using LittleArkFoundation.Areas.Admin.Models.FosterCare;
using LittleArkFoundation.Areas.Admin.Models.GeneralAdmission;
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
using LittleArkFoundation.Areas.Admin.Models.OPD;
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
using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq.Expressions;
using System.Security.Claims;

namespace LittleArkFoundation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [HasPermission("ManageDischarge")]
    public class DischargeController : Controller
    {
        private readonly ConnectionService _connectionService;
        public DischargeController(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<IActionResult> Index()
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();
            var discharges = await context.Discharges.OrderByDescending(d => d.DischargedDate).ToListAsync();

            var viewModel = new DischargeViewModel
            {
                Users = users,
                Discharges = discharges,
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Search(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
            {
                return RedirectToAction("Index");
            }

            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            var searchWords = searchString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var query = context.Discharges
                .OrderByDescending(d => d.DischargedDate)
                .AsQueryable();

            foreach (var word in searchWords)
            {
                var term = word.Trim();

                query = query.Where(u =>
                    EF.Functions.Like(u.FirstName, $"%{term}%") ||
                    EF.Functions.Like(u.MiddleName, $"%{term}%") ||
                    EF.Functions.Like(u.LastName, $"%{term}%"));
            }

            var discharges = await query.ToListAsync();

            var viewModel = new DischargeViewModel
            {
                Users = users,
                Discharges = discharges,
            };

            return View("Index", viewModel);
        }

        public async Task<IActionResult> SortBy(string sortByUserID, string? sortByMonth)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var query = context.Discharges.AsQueryable();

            if (!string.IsNullOrEmpty(sortByUserID))
            {
                query = query.Where(d => d.UserID == int.Parse(sortByUserID));
                var user = await context.Users.FindAsync(int.Parse(sortByUserID));
                ViewBag.sortBy = user.Username;
                ViewBag.sortByUserID = user.UserID.ToString();
            }

            if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime month))
            {
                query = query.Where(d => d.DischargedDate.Month == month.Month && d.DischargedDate.Year == month.Year);
                ViewBag.sortByMonth = month.ToString("yyyy-MM");
            }

            var discharges = await query.ToListAsync();

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            var viewModel = new DischargeViewModel
            {
                Discharges = discharges,
                Users = users
            };

            return View("Index", viewModel);
        }

        public async Task<IActionResult> SortbyReports(string sortByUserID, string? sortByMonth, string? viewName = "Index")
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var query = context.Discharges.AsQueryable();

            if (!string.IsNullOrEmpty(sortByUserID))
            {
                query = query.Where(d => d.UserID == int.Parse(sortByUserID));
                var user = await context.Users.FindAsync(int.Parse(sortByUserID));
                ViewBag.sortBy = user.Username;
                ViewBag.sortByUserID = user.UserID.ToString();
            }

            if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime month))
            {
                query = query.Where(d => d.DischargedDate.Month == month.Month && d.DischargedDate.Year == month.Year);
                ViewBag.sortByMonth = month.ToString("yyyy-MM");
            }

            var discharges = await query.ToListAsync();

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            var viewModel = new DischargeViewModel
            {
                Discharges = discharges,
                Users = users
            };

            return View(viewName, viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DischargePatient(PatientsViewModel patientsViewModel)
        {
            try
            {
                string connectionString = _connectionService.GetCurrentConnectionString();
                await using var context = new ApplicationDbContext(connectionString);

                var discharge = patientsViewModel.Discharge;
                var patient = await context.Patients.FindAsync(discharge.PatientID);

                // Example: Map data to a discharge entity
                var dischargeEntity = new DischargesModel
                {
                    PatientID = discharge.PatientID,
                    AssessmentID = discharge.AssessmentID,
                    FirstName = discharge.FirstName,
                    MiddleName = discharge.MiddleName,
                    LastName = discharge.LastName,
                    Ward = discharge.Ward,
                    Class = discharge.Class,
                    ProcessedDate = discharge.ProcessedDate,
                    DischargedDate = discharge.DischargedDate,
                    ReceivedHB = discharge.ReceivedHB,
                    IssuedMSS = discharge.IssuedMSS,
                    Duration = discharge.Duration,
                    PHICCategory = discharge.PHICCategory,
                    PHICUsed = discharge.PHICUsed,
                    MSW = discharge.MSW,
                    RemarksIfNo = discharge.RemarksIfNo,
                    UserID = discharge.UserID,
                };

                patient.IsActive = false; // Mark patient as inactive

                context.Discharges.Add(dischargeEntity);
                context.Patients.Update(patient);
                await context.SaveChangesAsync();

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                TempData["SuccessMessage"] = "Patient successfully discharged.";
                LoggingService.LogInformation($"Patient discharge successful. Discharged PatientID: {discharge.PatientID}. Discharged by UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");
                return RedirectToAction("Index", "Form");
            }
            catch (SqlException se)
            {
                TempData["ErrorMessage"] = "SQL Error: " + se.Message;
                LoggingService.LogError("SQL Error: " + se.Message);
                return RedirectToAction("Index", "Form");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                LoggingService.LogError("Error: " + ex.Message);
                return RedirectToAction("Index", "Form");
            }
        }

        public async Task<IActionResult> ReAdmitPatient(int id, int assessmentID)
        {
            try
            {
                string connectionString = _connectionService.GetCurrentConnectionString();

                await using (var context = new ApplicationDbContext(connectionString))
                {
                    if (!ModelState.IsValid)
                    {
                        TempData["ErrorMessage"] = $"Invalid data submitted.";
                        return RedirectToAction("Index", "Form", new { isActive = false });
                    }

                    var patientID = id;
                    var newAssessmentID = await new AssessmentsRepository(connectionString).GenerateID(id);
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                    // ASSESSMENTS
                    var assessment = await context.Assessments.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newAssessment = new AssessmentsModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        Age = assessment.Age,
                        DateOfInterview = DateOnly.FromDateTime(DateTime.Now),
                        TimeOfInterview = TimeOnly.FromDateTime(DateTime.Now),
                        BasicWard = assessment.BasicWard,
                        NonBasicWard = assessment.NonBasicWard,
                        HealthRecordNo = assessment.HealthRecordNo,
                        MSWDNo = assessment.MSWDNo,
                        AssessmentStatement = assessment.AssessmentStatement,
                        UserID = int.Parse(userIdClaim.Value)
                    };

                    // REFERRALS
                    var referral = await context.Referrals.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newReferral = new ReferralsModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        ReferralType = referral.ReferralType,
                        Name = referral.Name,
                        Address = referral.Address,
                        ContactNo = referral.ContactNo,
                        DateOfReferral = DateTime.Now
                    };

                    // INFORMANTS
                    var informant = await context.Informants.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newInformant = new InformantsModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        Name = informant.Name,
                        RelationToPatient = informant.RelationToPatient,
                        ContactNo = informant.ContactNo,
                        Address = informant.Address,
                        DateOfInformant = DateTime.Now
                    };

                    // PATIENTS
                    var patient = await context.Patients.FirstOrDefaultAsync(s => s.PatientID == id);

                    // FAMILY COMPOSITION
                    var familyComposition = await context.FamilyComposition
                        .Where(s => s.AssessmentID == assessmentID && s.PatientID == patientID)
                        .ToListAsync();

                    var newFamilyComposition = new List<FamilyCompositionModel>();

                    foreach (var familyMember in familyComposition)
                    {
                        var newMember = new FamilyCompositionModel
                        {
                            AssessmentID = newAssessmentID,
                            PatientID = patientID,
                            Name = familyMember.Name,
                            Age = familyMember.Age,
                            DateOfBirth = familyMember.DateOfBirth,
                            CivilStatus = familyMember.CivilStatus,
                            RelationshipToPatient = familyMember.RelationshipToPatient,
                            LivingWithChild = familyMember.LivingWithChild,
                            EducationalAttainment = familyMember.EducationalAttainment,
                            Occupation = familyMember.Occupation,
                            MonthlyIncome = familyMember.MonthlyIncome
                        };

                        newFamilyComposition.Add(newMember);
                    }

                    // HOUSEHOLD
                    var household = await context.Households.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newHousehold = new HouseholdModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        OtherSourcesOfIncome = household.OtherSourcesOfIncome,
                        HouseholdSize = household.HouseholdSize,
                        TotalHouseholdIncome = household.TotalHouseholdIncome,
                        PerCapitaIncome = household.PerCapitaIncome
                    };

                    // MSWD CLASSIFICATION
                    var mswdclassification = await context.MSWDClassification.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newMSWDClassification = new MSWDClassificationModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        MainClassification = mswdclassification.MainClassification,
                        SubClassification = mswdclassification.SubClassification,
                        MembershipSector = mswdclassification.MembershipSector

                    };

                    // MONTHLY EXPENSES & UTILITIES
                    var monthlyExpenses = await context.MonthlyExpenses.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newMonthlyExpenses = new MonthlyExpensesModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        HouseAndLot = monthlyExpenses.HouseAndLot,
                        FoodAndWater = monthlyExpenses.FoodAndWater,
                        Education = monthlyExpenses.Education,
                        Clothing = monthlyExpenses.Clothing,
                        Communication = monthlyExpenses.Communication,
                        HouseHelp = monthlyExpenses.HouseHelp,
                        MedicalExpenses = monthlyExpenses.MedicalExpenses,
                        Transportation = monthlyExpenses.Transportation,
                        Others = monthlyExpenses.Others,
                        OthersAmount = monthlyExpenses.OthersAmount,
                        Total = monthlyExpenses.Total
                    };

                    var utilities = await context.Utilities.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newUtilities = new UtilitiesModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        LightSource = utilities.LightSource,
                        LightSourceAmount = utilities.LightSourceAmount,
                        FuelSource = utilities.FuelSource,
                        FuelSourceAmount = utilities.FuelSourceAmount,
                        WaterSource = utilities.WaterSource
                    };

                    // MEDICAL HISTORY
                    var medicalHistory = await context.MedicalHistory.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newMedicalHistory = new MedicalHistoryModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        AdmittingDiagnosis = medicalHistory.AdmittingDiagnosis,
                        FinalDiagnosis = medicalHistory.FinalDiagnosis,
                        DurationSymptomsPriorAdmission = medicalHistory.DurationSymptomsPriorAdmission,
                        PreviousTreatmentDuration = medicalHistory.PreviousTreatmentDuration,
                        TreatmentPlan = medicalHistory.TreatmentPlan,
                        HealthAccessibilityProblems = medicalHistory.HealthAccessibilityProblems
                    };

                    // CHILD HEALTH
                    var childHealth = await context.ChildHealth.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newChildHealth = new ChildHealthModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        OverallHealth = childHealth.OverallHealth,
                        HasHealthIssues = childHealth.HasHealthIssues,
                        DescribeHealthIssues = childHealth.DescribeHealthIssues,
                        HasRecurrentConditions = childHealth.HasRecurrentConditions,
                        DescribeRecurrentConditions = childHealth.DescribeRecurrentConditions,
                        HasEarTubes = childHealth.HasEarTubes
                    };

                    // DIAGNOSES
                    var diagnoses = await context.Diagnoses
                        .Where(s => s.AssessmentID == assessmentID && s.PatientID == patientID)
                        .ToListAsync();

                    var newDiagnoses = new List<DiagnosesModel>();

                    foreach (var diagnosis in diagnoses)
                    {
                        var newDiagnosis = new DiagnosesModel
                        {
                            AssessmentID = newAssessmentID,
                            PatientID = patientID,
                            MedicalCondition = diagnosis.MedicalCondition,
                            ReceivingTreatment = diagnosis.ReceivingTreatment,
                            TreatmentProvider = diagnosis.TreatmentProvider,
                            DoesCauseStressOrImpairment = diagnosis.DoesCauseStressOrImpairment,
                            TreatmentHelp = diagnosis.TreatmentHelp
                        };

                        newDiagnoses.Add(newDiagnosis);
                    }

                    // MEDICATIONS
                    var medications = await context.Medications
                        .Where(s => s.AssessmentID == assessmentID && s.PatientID == patientID)
                        .ToListAsync();

                    var newMedications = new List<MedicationsModel>();

                    foreach (var medication in medications)
                    {
                        var newMedication = new MedicationsModel
                        {
                            AssessmentID = newAssessmentID,
                            PatientID = patientID,
                            DoesTakeAnyMedication = medication.DoesTakeAnyMedication,
                            Medication = medication.Medication,
                            Dosage = medication.Dosage,
                            Frequency = medication.Frequency,
                            PrescribedBy = medication.PrescribedBy,
                            ReasonForMedication = medication.ReasonForMedication,
                            IsTakingMedicationAsPrescribed = medication.IsTakingMedicationAsPrescribed,
                            DescribeTakingMedication = medication.DescribeTakingMedication,
                            AdditionalInfo = medication.AdditionalInfo
                        };

                        newMedications.Add(newMedication);
                    }

                    // HOSPITALIZATION HISTORY
                    var hospitalizationHistory = await context.HospitalizationHistory
                        .Where(s => s.AssessmentID == assessmentID && s.PatientID == patientID)
                        .ToListAsync();

                    var newHospitalizationHistory = new List<HospitalizationHistoryModel>();

                    foreach (var hospitalization in hospitalizationHistory)
                    {
                        var newHospitalization = new HospitalizationHistoryModel
                        {
                            AssessmentID = newAssessmentID,
                            PatientID = patientID,
                            HasSeriousAccidentOrIllness = hospitalization.HasSeriousAccidentOrIllness,
                            Reason = hospitalization.Reason,
                            Date = hospitalization.Date,
                            Location = hospitalization.Location
                        };

                        newHospitalizationHistory.Add(newHospitalization);
                    }

                    // MEDICAL SCREENINGS
                    var medicalscreenings = await context.MedicalScreenings.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newMedicalScreenings = new MedicalScreeningsModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        HasScreeningDone = medicalscreenings.HasScreeningDone,
                        HearingTestDate = medicalscreenings.HearingTestDate,
                        HearingTestOutcome = medicalscreenings.HearingTestOutcome,
                        VisionTestDate = medicalscreenings.VisionTestDate,
                        VisionTestOutcome = medicalscreenings.VisionTestOutcome,
                        SpeechTestDate = medicalscreenings.SpeechTestDate,
                        SpeechTestOutcome = medicalscreenings.SpeechTestOutcome
                    };

                    // PRIMARY CARE DOCTOR
                    var primaryCareDoctor = await context.PrimaryCareDoctor.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newPrimaryCareDoctor = new PrimaryCareDoctorModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        DoctorName = primaryCareDoctor.DoctorName,
                        Facility = primaryCareDoctor.Facility,
                        PhoneNumber = primaryCareDoctor.PhoneNumber
                    };

                    // PRESENTING PROBLEMS
                    var presentingproblems = await context.PresentingProblems.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newPresentingProblems = new PresentingProblemsModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        PresentingProblem = presentingproblems.PresentingProblem,
                        Severity = presentingproblems.Severity,
                        ChangeInSleepPattern = presentingproblems.ChangeInSleepPattern,
                        Concentration = presentingproblems.Concentration,
                        ChangeInAppetite = presentingproblems.ChangeInAppetite,
                        IncreasedAnxiety = presentingproblems.IncreasedAnxiety,
                        MoodSwings = presentingproblems.MoodSwings,
                        BehavioralChanges = presentingproblems.BehavioralChanges,
                        Victimization = presentingproblems.Victimization,
                        DescribeOtherConcern = presentingproblems.DescribeOtherConcern,
                        DurationOfStress = presentingproblems.DurationOfStress,
                        CopingLevel = presentingproblems.CopingLevel,
                        OtherFamilySituation = presentingproblems.OtherFamilySituation
                    };

                    // RECENT LOSSES
                    var recentlosses = await context.RecentLosses.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newRecentLosses = new RecentLossesModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        FamilyMemberLoss = recentlosses.FamilyMemberLoss,
                        FriendLoss = recentlosses.FriendLoss,
                        HealthLoss = recentlosses.HealthLoss,
                        LifestyleLoss = recentlosses.LifestyleLoss,
                        JobLoss = recentlosses.JobLoss,
                        IncomeLoss = recentlosses.IncomeLoss,
                        HousingLoss = recentlosses.HousingLoss,
                        NoneLoss = recentlosses.NoneLoss,
                        Name = recentlosses.Name,
                        Date = recentlosses.Date,
                        NatureOfLoss = recentlosses.NatureOfLoss,
                        OtherLosses = recentlosses.OtherLosses,
                        AdditionalInfo = recentlosses.AdditionalInfo
                    };

                    // PREGNANCY BIRTH HISTORY
                    var pregnancyBirthHistory = await context.PregnancyBirthHistory.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newPregnancyBirthHistory = new PregnancyBirthHistoryModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        HasPregnancyComplications = pregnancyBirthHistory.HasPregnancyComplications,
                        DescribePregnancyComplications = pregnancyBirthHistory.DescribePregnancyComplications,
                        IsFullTermBirth = pregnancyBirthHistory.IsFullTermBirth,
                        HasBirthComplications = pregnancyBirthHistory.HasBirthComplications,
                        DescribeBirthComplications = pregnancyBirthHistory.DescribeBirthComplications,
                        HasConsumedDrugs = pregnancyBirthHistory.HasConsumedDrugs,
                        BirthWeightLbs = pregnancyBirthHistory.BirthWeightLbs,
                        BirthWeightOz = pregnancyBirthHistory.BirthWeightOz,
                        BirthHealth = pregnancyBirthHistory.BirthHealth,
                        LengthOfHospitalStay = pregnancyBirthHistory.LengthOfHospitalStay,
                        PostpartumDepression = pregnancyBirthHistory.PostpartumDepression,
                        WasChildAdopted = pregnancyBirthHistory.WasChildAdopted,
                        ChildAdoptedAge = pregnancyBirthHistory.ChildAdoptedAge,
                        AdoptionType = pregnancyBirthHistory.AdoptionType,
                        AdoptionCountry = pregnancyBirthHistory.AdoptionCountry
                    };

                    // DEVELOPMENTAL HISTORY
                    var developmentalhistory = await context.DevelopmentalHistory.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newDevelopmentalHistory = new DevelopmentalHistoryModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        RolledOverAge = developmentalhistory.RolledOverAge,
                        CrawledAge = developmentalhistory.CrawledAge,
                        WalkedAge = developmentalhistory.WalkedAge,
                        TalkedAge = developmentalhistory.TalkedAge,
                        ToiletTrainedAge = developmentalhistory.ToiletTrainedAge,
                        SpeechConcerns = developmentalhistory.SpeechConcerns,
                        MotorSkillsConcerns = developmentalhistory.MotorSkillsConcerns,
                        CognitiveConcerns = developmentalhistory.CognitiveConcerns,
                        SensoryConcerns = developmentalhistory.SensoryConcerns,
                        BehavioralConcerns = developmentalhistory.BehavioralConcerns,
                        EmotionalConcerns = developmentalhistory.EmotionalConcerns,
                        SocialConcerns = developmentalhistory.SocialConcerns,
                        HasSignificantDisturbance = developmentalhistory.HasSignificantDisturbance,
                        DescribeSignificantDisturbance = developmentalhistory.DescribeSignificantDisturbance
                    };

                    // MENTAL HEALTH HISTORY
                    var mentalHealthHistory = await context.MentalHealthHistory
                        .Where(s => s.AssessmentID == assessmentID && s.PatientID == patientID)
                        .ToListAsync();

                    var newMentalHealthHistory = new List<MentalHealthHistoryModel>();

                    foreach (var mentalHealth in mentalHealthHistory)
                    {
                        var newMentalHealth = new MentalHealthHistoryModel
                        {
                            AssessmentID = newAssessmentID,
                            PatientID = patientID,
                            HasReceivedCounseling = mentalHealth.HasReceivedCounseling,
                            DateOfService = mentalHealth.DateOfService,
                            Provider = mentalHealth.Provider,
                            ReasonForTreatment = mentalHealth.ReasonForTreatment,
                            WereServicesHelpful = mentalHealth.WereServicesHelpful
                        };

                        newMentalHealthHistory.Add(newMentalHealth);
                    }

                    // FAMILY HISTORY   
                    var familyHistory = await context.FamilyHistory
                        .Where(s => s.AssessmentID == assessmentID && s.PatientID == patientID)
                        .ToListAsync();

                    var newFamilyHistory = new List<FamilyHistoryModel>();

                    foreach (var history in familyHistory)
                    {
                        var newHistory = new FamilyHistoryModel
                        {
                            AssessmentID = newAssessmentID,
                            PatientID = patientID,
                            IsSelf = history.IsSelf,
                            HasDepression = history.HasDepression,
                            HasAnxiety = history.HasAnxiety,
                            HasBipolarDisorder = history.HasBipolarDisorder,
                            HasSchizophrenia = history.HasSchizophrenia,
                            HasADHD_ADD = history.HasADHD_ADD,
                            HasTraumaHistory = history.HasTraumaHistory,
                            HasAbusiveBehavior = history.HasAbusiveBehavior,
                            HasAlcoholAbuse = history.HasAlcoholAbuse,
                            HasDrugAbuse = history.HasDrugAbuse,
                            HasIncarceration = history.HasIncarceration,
                            AdditionalInfo = history.AdditionalInfo

                        };

                        newFamilyHistory.Add(newHistory);
                    }

                    // SAFETY CONCERNS
                    var safetyConcerns = await context.SafetyConcerns.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newSafetyConcerns = new SafetyConcernsModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        IsSuicidal = safetyConcerns.IsSuicidal,
                        DescribeSuicidal = safetyConcerns.DescribeSuicidal,
                        HasAttemptedSuicide = safetyConcerns.HasAttemptedSuicide,
                        DescribeAttemptedSuicide = safetyConcerns.DescribeAttemptedSuicide,
                        IsThereHistoryOfSuicide = safetyConcerns.IsThereHistoryOfSuicide,
                        DescribeHistoryOfSuicide = safetyConcerns.DescribeHistoryOfSuicide,
                        HasSelfHarm = safetyConcerns.HasSelfHarm,
                        IsHomicidal = safetyConcerns.IsHomicidal,
                        DescribeHomicidal = safetyConcerns.DescribeHomicidal,
                        AdditionalInfo = safetyConcerns.AdditionalInfo
                    };

                    // CURRENT FUNCTIONING
                    var currentFunctioning = await context.CurrentFunctioning.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newCurrentFunctioning = new CurrentFunctioningModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        EatingConcerns = currentFunctioning.EatingConcerns,
                        HygieneConcerns = currentFunctioning.HygieneConcerns,
                        SleepingConcerns = currentFunctioning.SleepingConcerns,
                        ActivitiesConcerns = currentFunctioning.ActivitiesConcerns,
                        SocialRelationshipsConcerns = currentFunctioning.SocialRelationshipsConcerns,
                        DescribeConcerns = currentFunctioning.DescribeConcerns,
                        EnergyLevel = currentFunctioning.EnergyLevel,
                        PhysicalLevel = currentFunctioning.PhysicalLevel,
                        AnxiousLevel = currentFunctioning.AnxiousLevel,
                        HappyLevel = currentFunctioning.HappyLevel,
                        CuriousLevel = currentFunctioning.CuriousLevel,
                        AngryLevel = currentFunctioning.AngryLevel,
                        IntensityLevel = currentFunctioning.IntensityLevel,
                        PersistenceLevel = currentFunctioning.PersistenceLevel,
                        SensitivityLevel = currentFunctioning.SensitivityLevel,
                        PerceptivenessLevel = currentFunctioning.PerceptivenessLevel,
                        AdaptabilityLevel = currentFunctioning.AdaptabilityLevel,
                        AttentionSpanLevel = currentFunctioning.AttentionSpanLevel
                    };

                    // PARENT CHILD RELATIONSHIP
                    var parentchildrelationship = await context.ParentChildRelationship.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newParentChildRelationship = new ParentChildRelationshipModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        ParentingExperience = parentchildrelationship.ParentingExperience,
                        Challenges = parentchildrelationship.Challenges,
                        DisciplineMethods = parentchildrelationship.DisciplineMethods
                    };

                    // EDUCATION
                    var education = await context.Education.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newEducation = new EducationModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        IsCurrentlyEnrolled = education.IsCurrentlyEnrolled,
                        SchoolName = education.SchoolName,
                        ChildGradeLevel = education.ChildGradeLevel,
                        SummerGradeLevel = education.SummerGradeLevel,
                        DescribeChildAttendance = education.DescribeChildAttendance,
                        ChildAttendance = education.ChildAttendance,
                        DescribeChildAchievements = education.DescribeChildAchievements,
                        DescribeChildAttitude = education.DescribeChildAttitude,
                        HasDisciplinaryIssues = education.HasDisciplinaryIssues,
                        DescribeDisciplinaryIssues = education.DescribeDisciplinaryIssues,
                        HasSpecialEducation = education.HasSpecialEducation,
                        DescribeSpecialEducation = education.DescribeSpecialEducation,
                        HasHomeStudy = education.HasHomeStudy,
                        DescribeHomeStudy = education.DescribeHomeStudy,
                        HasDiagnosedLearningDisability = education.HasDiagnosedLearningDisability,
                        DescribeDiagnosedLearningDisability = education.DescribeDiagnosedLearningDisability,
                        HasSpecialServices = education.HasSpecialServices,
                        DescribeSpecialServices = education.DescribeSpecialServices
                    };

                    // EMPLOYMENT
                    var employment = await context.Employment.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newEmployment = new EmploymentModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        IsCurrentlyEmployed = employment.IsCurrentlyEmployed,
                        Location = employment.Location,
                        JobDuration = employment.JobDuration,
                        IsEnjoyingJob = employment.IsEnjoyingJob
                    };

                    // HOUSING
                    var housing = await context.Housing.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newHousing = new HousingModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        IsStable = housing.IsStable,
                        DescribeIfUnstable = housing.DescribeIfUnstable,
                        HousingType = housing.HousingType,
                        DurationLivedInHouse = housing.DurationLivedInHouse,
                        TimesMoved = housing.TimesMoved,
                        AdditionalInfo = housing.AdditionalInfo
                    };

                    // FOSTER CARE
                    var fosterCare = await context.FosterCare.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newFosterCare = new FosterCareModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        HasBeenFosterCared = fosterCare.HasBeenFosterCared,
                        FosterAgeStart = fosterCare.FosterAgeStart,
                        FosterAgeEnd = fosterCare.FosterAgeEnd,
                        Reason = fosterCare.Reason,
                        PlacementType = fosterCare.PlacementType,
                        CurrentStatus = fosterCare.CurrentStatus,
                        OutOfCareReason = fosterCare.OutOfCareReason
                    };

                    // ALCOHOL DRUG ASSESSMENT
                    var alcoholDrugAssessment = await context.AlcoholDrugAssessment.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newAlcoholDrugAssessment = new AlcoholDrugAssessmentModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        TobaccoUse = alcoholDrugAssessment.TobaccoUse,
                        AlcoholUse = alcoholDrugAssessment.AlcoholUse,
                        RecreationalMedicationUse = alcoholDrugAssessment.RecreationalMedicationUse,
                        HasOverdosed = alcoholDrugAssessment.HasOverdosed,
                        OverdoseDate = alcoholDrugAssessment.OverdoseDate,
                        HasAlcoholProblems = alcoholDrugAssessment.HasAlcoholProblems,
                        LegalProblems = alcoholDrugAssessment.LegalProblems,
                        SocialPeerProblems = alcoholDrugAssessment.SocialPeerProblems,
                        WorkProblems = alcoholDrugAssessment.WorkProblems,
                        FamilyProblems = alcoholDrugAssessment.FamilyProblems,
                        FriendsProblems = alcoholDrugAssessment.FriendsProblems,
                        FinancialProblems = alcoholDrugAssessment.FinancialProblems,
                        DescribeProblems = alcoholDrugAssessment.DescribeProblems,
                        ContinuedUse = alcoholDrugAssessment.ContinuedUse
                    };

                    // LEGAL INVOLVEMENT
                    var legalInvolvement = await context.LegalInvolvement.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newLegalInvolvement = new LegalInvolvementModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        HasCustodyCase = legalInvolvement.HasCustodyCase,
                        DescribeCustodyCase = legalInvolvement.DescribeCustodyCase,
                        HasCPSInvolvement = legalInvolvement.HasCPSInvolvement,
                        DescribeCPSInvolvement = legalInvolvement.DescribeCPSInvolvement,
                        LegalStatus = legalInvolvement.LegalStatus,
                        ProbationParoleLength = legalInvolvement.ProbationParoleLength,
                        Charges = legalInvolvement.Charges,
                        OfficerName = legalInvolvement.OfficerName,
                        OfficerContactNum = legalInvolvement.OfficerContactNum,
                        AdditionalInfo = legalInvolvement.AdditionalInfo
                    };

                    // HISTORY OF ABUSE
                    var historyOfAbuse = await context.HistoryOfAbuse.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newHistoryOfAbuse = new HistoryOfAbuseModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        HasBeenAbused = historyOfAbuse.HasBeenAbused,
                        SexualAbuse = historyOfAbuse.SexualAbuse,
                        SexualAbuseByWhom = historyOfAbuse.SexualAbuseByWhom,
                        SexualAbuseAgeOfChild = historyOfAbuse.SexualAbuseAgeOfChild,
                        SexualAbuseReported = historyOfAbuse.SexualAbuseReported,
                        PhysicalAbuse = historyOfAbuse.PhysicalAbuse,
                        PhysicalAbuseByWhom = historyOfAbuse.PhysicalAbuseByWhom,
                        PhysicalAbuseAgeOfChild = historyOfAbuse.PhysicalAbuseAgeOfChild,
                        PhysicalAbuseReported = historyOfAbuse.PhysicalAbuseReported,
                        EmotionalAbuse = historyOfAbuse.EmotionalAbuse,
                        EmotionalAbuseByWhom = historyOfAbuse.EmotionalAbuseByWhom,
                        EmotionalAbuseAgeOfChild = historyOfAbuse.EmotionalAbuseAgeOfChild,
                        EmotionalAbuseReported = historyOfAbuse.EmotionalAbuseReported,
                        VerbalAbuse = historyOfAbuse.VerbalAbuse,
                        VerbalAbuseByWhom = historyOfAbuse.VerbalAbuseByWhom,
                        VerbalAbuseAgeOfChild = historyOfAbuse.VerbalAbuseAgeOfChild,
                        VerbalAbuseReported = historyOfAbuse.VerbalAbuseReported,
                        AbandonedAbuse = historyOfAbuse.AbandonedAbuse,
                        AbandonedAbuseByWhom = historyOfAbuse.AbandonedAbuseByWhom,
                        AbandonedAbuseAgeOfChild = historyOfAbuse.AbandonedAbuseAgeOfChild,
                        AbandonedAbuseReported = historyOfAbuse.AbandonedAbuseReported,
                        PsychologicalAbuse = historyOfAbuse.PsychologicalAbuse,
                        PsychologicalAbuseByWhom = historyOfAbuse.PsychologicalAbuseByWhom,
                        PsychologicalAbuseAgeOfChild = historyOfAbuse.PsychologicalAbuseAgeOfChild,
                        PsychologicalAbuseReported = historyOfAbuse.PsychologicalAbuseReported,
                        VictimOfBullying = historyOfAbuse.VictimOfBullying,
                        SafetyConcerns = historyOfAbuse.SafetyConcerns,
                        DescribeSafetyConcerns = historyOfAbuse.DescribeSafetyConcerns,
                        AdditionalInfo = historyOfAbuse.AdditionalInfo
                    };

                    // HISTORY OF VIOLENCE
                    var historyOfViolence = await context.HistoryOfViolence.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newHistoryOfViolence = new HistoryOfViolenceModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        HasBeenAccused = historyOfViolence.HasBeenAccused,
                        SexualAbuse = historyOfViolence.SexualAbuse,
                        SexualAbuseToWhom = historyOfViolence.SexualAbuseToWhom,
                        SexualAbuseAgeOfChild = historyOfViolence.SexualAbuseAgeOfChild,
                        SexualAbuseReported = historyOfViolence.SexualAbuseReported,
                        PhysicalAbuse = historyOfViolence.PhysicalAbuse,
                        PhysicalAbuseToWhom = historyOfViolence.PhysicalAbuseToWhom,
                        PhysicalAbuseAgeOfChild = historyOfViolence.PhysicalAbuseAgeOfChild,
                        PhysicalAbuseReported = historyOfViolence.PhysicalAbuseReported,
                        EmotionalAbuse = historyOfViolence.EmotionalAbuse,
                        EmotionalAbuseToWhom = historyOfViolence.EmotionalAbuseToWhom,
                        EmotionalAbuseAgeOfChild = historyOfViolence.EmotionalAbuseAgeOfChild,
                        EmotionalAbuseReported = historyOfViolence.EmotionalAbuseReported,
                        VerbalAbuse = historyOfViolence.VerbalAbuse,
                        VerbalAbuseToWhom = historyOfViolence.VerbalAbuseToWhom,
                        VerbalAbuseAgeOfChild = historyOfViolence.VerbalAbuseAgeOfChild,
                        VerbalAbuseReported = historyOfViolence.VerbalAbuseReported,
                        AbandonedAbuse = historyOfViolence.AbandonedAbuse,
                        AbandonedAbuseToWhom = historyOfViolence.AbandonedAbuseToWhom,
                        AbandonedAbuseAgeOfChild = historyOfViolence.AbandonedAbuseAgeOfChild,
                        AbandonedAbuseReported = historyOfViolence.AbandonedAbuseReported,
                        Bullying = historyOfViolence.Bullying,
                        AdditionalInfo = historyOfViolence.AdditionalInfo
                    };

                    // STRENGTHS RESOURCES
                    var strengthsResources = await context.StrengthsResources.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newStrengthsResources = new StrengthsResourcesModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        Strengths = strengthsResources.Strengths,
                        Limitations = strengthsResources.Limitations,
                        Resources = strengthsResources.Resources,
                        Experiences = strengthsResources.Experiences,
                        AlreadyDoing = strengthsResources.AlreadyDoing,
                        ParentsSupport = strengthsResources.ParentsSupport,
                        PartnerSupport = strengthsResources.PartnerSupport,
                        SiblingsSupport = strengthsResources.SiblingsSupport,
                        ExtendedFamilySupport = strengthsResources.ExtendedFamilySupport,
                        FriendsSupport = strengthsResources.FriendsSupport,
                        NeighborsSupport = strengthsResources.NeighborsSupport,
                        SchoolStaffSupport = strengthsResources.SchoolStaffSupport,
                        ChurchSupport = strengthsResources.ChurchSupport,
                        PastorSupport = strengthsResources.PastorSupport,
                        TherapistSupport = strengthsResources.TherapistSupport,
                        GroupSupport = strengthsResources.GroupSupport,
                        CommunityServiceSupport = strengthsResources.CommunityServiceSupport,
                        DoctorSupport = strengthsResources.DoctorSupport,
                        OthersSupport = strengthsResources.OthersSupport,
                        Others = strengthsResources.Others
                    };

                    // GOALS
                    var goals = await context.Goals.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newGoals = new GoalsModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        CurrentNeeds = goals.CurrentNeeds,
                        HopeToGain = goals.HopeToGain,
                        Goal1 = goals.Goal1,
                        Goal2 = goals.Goal2,
                        Goal3 = goals.Goal3,
                        AdditionalInfo = goals.AdditionalInfo
                    };

                    // PROGRESS NOTES
                    var progressnotes = await context.ProgressNotes
                        .Where(s => s.AssessmentID == assessmentID && s.PatientID == patientID)
                        .ToListAsync();
                    var newProgressNotes = new List<ProgressNotesModel>();
                    foreach (var progressNote in progressnotes)
                    {
                        var newProgressNote = new ProgressNotesModel
                        {
                            AssessmentID = newAssessmentID,
                            PatientID = patientID,
                            Date = progressNote.Date,
                            ProgressNotes = progressNote.ProgressNotes,
                            Attachment = progressNote.Attachment,
                            AttachmentContentType = progressNote.AttachmentContentType,
                            UserID = progressNote.UserID
                        };
                        newProgressNotes.Add(newProgressNote);
                    }

                    // GENERAL ADMISSION
                    var generalAdmission = await context.GeneralAdmission.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == patientID);
                    var newGeneralAdmission = new GeneralAdmissionModel
                    {
                        AssessmentID = newAssessmentID,
                        PatientID = patientID,
                        Date = DateOnly.FromDateTime(DateTime.Now),
                        isOld = true,
                        HospitalNo = generalAdmission.HospitalNo,
                        FirstName = generalAdmission.FirstName,
                        MiddleName = generalAdmission.MiddleName,
                        LastName = generalAdmission.LastName,
                        Ward = generalAdmission.Ward,
                        Class = generalAdmission.Class,
                        Age = generalAdmission.Age,
                        Gender = generalAdmission.Gender,
                        Time = TimeOnly.FromDateTime(DateTime.Now),
                        Diagnosis = generalAdmission.Diagnosis,
                        CompleteAddress = generalAdmission.CompleteAddress,
                        Origin = generalAdmission.Origin,
                        ContactNumber = generalAdmission.ContactNumber,
                        Referral = generalAdmission.Referral,
                        Occupation = generalAdmission.Occupation,
                        StatsOccupation = generalAdmission.StatsOccupation,
                        IncomeRange = generalAdmission.IncomeRange,
                        MonthlyIncome = generalAdmission.MonthlyIncome,
                        EconomicStatus = generalAdmission.EconomicStatus,
                        HouseholdSize = generalAdmission.HouseholdSize,
                        MaritalStatus = generalAdmission.MaritalStatus,
                        isPWD = generalAdmission.isPWD,
                        EducationalAttainment = generalAdmission.EducationalAttainment,
                        FatherEducationalAttainment = generalAdmission.FatherEducationalAttainment,
                        MotherEducationalAttainment = generalAdmission.MotherEducationalAttainment,
                        isInterviewed = generalAdmission.isInterviewed,
                        DwellingType = generalAdmission.DwellingType,
                        LightSource = generalAdmission.LightSource,
                        WaterSource = generalAdmission.WaterSource,
                        FuelSource = generalAdmission.FuelSource,
                        PHIC = generalAdmission.PHIC,
                        MSW = User.FindFirstValue(ClaimTypes.Name),
                        UserID = int.Parse(userIdClaim.Value)
                    };

                    // Change patient to active
                    patient.IsActive = true;

                    await context.Assessments.AddAsync(newAssessment);
                    await context.SaveChangesAsync();

                    // Update the rest of the form
                    await context.Referrals.AddAsync(newReferral);
                    await context.Informants.AddAsync(newInformant);
                    await context.FamilyComposition.AddRangeAsync(newFamilyComposition);
                    await context.Households.AddAsync(newHousehold);
                    await context.MSWDClassification.AddAsync(newMSWDClassification);
                    await context.MonthlyExpenses.AddAsync(newMonthlyExpenses);
                    await context.Utilities.AddAsync(newUtilities);
                    await context.MedicalHistory.AddAsync(newMedicalHistory);
                    await context.ChildHealth.AddAsync(newChildHealth);
                    await context.Diagnoses.AddRangeAsync(newDiagnoses);
                    await context.Medications.AddRangeAsync(newMedications);
                    await context.HospitalizationHistory.AddRangeAsync(newHospitalizationHistory);
                    await context.MedicalScreenings.AddAsync(newMedicalScreenings);
                    await context.PrimaryCareDoctor.AddAsync(newPrimaryCareDoctor);
                    await context.PresentingProblems.AddAsync(newPresentingProblems);
                    await context.RecentLosses.AddAsync(newRecentLosses);
                    await context.PregnancyBirthHistory.AddAsync(newPregnancyBirthHistory);
                    await context.DevelopmentalHistory.AddAsync(newDevelopmentalHistory);
                    await context.MentalHealthHistory.AddRangeAsync(newMentalHealthHistory);
                    await context.FamilyHistory.AddRangeAsync(newFamilyHistory);
                    await context.SafetyConcerns.AddAsync(newSafetyConcerns);
                    await context.CurrentFunctioning.AddAsync(newCurrentFunctioning);
                    await context.ParentChildRelationship.AddAsync(newParentChildRelationship);
                    await context.Education.AddAsync(newEducation);
                    await context.Employment.AddAsync(newEmployment);
                    await context.Housing.AddAsync(newHousing);
                    await context.FosterCare.AddAsync(newFosterCare);
                    await context.AlcoholDrugAssessment.AddAsync(newAlcoholDrugAssessment);
                    await context.LegalInvolvement.AddAsync(newLegalInvolvement);
                    await context.HistoryOfAbuse.AddAsync(newHistoryOfAbuse);
                    await context.HistoryOfViolence.AddAsync(newHistoryOfViolence);
                    await context.StrengthsResources.AddAsync(newStrengthsResources);
                    await context.Goals.AddAsync(newGoals);
                    await context.ProgressNotes.AddRangeAsync(newProgressNotes);
                    await context.GeneralAdmission.AddAsync(newGeneralAdmission);
                    await context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Successfully re-admitted patient.";
                    LoggingService.LogInformation($"Patient Re-admission successful. Created new AssessmentID: {newAssessmentID}. Re-admitted by UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");
                    return RedirectToAction("Edit", "Form", new { id = patientID, assessmentID = newAssessmentID });

                }
            }
            catch (SqlException se)
            {
                TempData["ErrorMessage"] = "SQL Error: " + se.Message;
                LoggingService.LogError("SQL Error: " + se.Message);
                return RedirectToAction("Index", "Form");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                LoggingService.LogError("Error: " + ex.Message);
                return RedirectToAction("Index", "Form");
            }
        }

        public async Task<IActionResult> ExportToExcel(int userID)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            List<DischargesModel> discharges;
            string fileName;

            if (userID == 0)
            {
                discharges = await context.Discharges.ToListAsync();
                fileName = $"Discharges_{discharges[0].DischargedDate.Year}_All";
            }
            else
            {
                discharges = await context.Discharges
                    .Where(d => d.UserID == userID)
                    .ToListAsync();

                if (discharges == null || !discharges.Any())
                {
                    TempData["ErrorMessage"] = "No discharges found for the selected user.";
                    return RedirectToAction("Index");
                }

                fileName = $"Discharges_{discharges[0].DischargedDate.Year}_{discharges[0].MSW}";
            }

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(fileName);

            // HEADERS
            // Column 1
            var cell1 = worksheet.Cell(1, 1);
            cell1.Value = userID == 0 ? "All" : discharges[0].MSW;
            cell1.Style.Font.Bold = true;
            cell1.Style.Font.FontSize = 14;
            cell1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range(1, 1, 1, 13).Merge(); // Merge across desired columns

            // Column 2
            var cell2 = worksheet.Cell(2, 1);
            cell2.Value = "LOG SHEET OF FACILITATED DISCHARGED PATIENTS WITH ZERO COPAYMENT";
            cell2.Style.Font.Bold = true;
            cell2.Style.Font.FontSize = 12;
            cell2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range(2, 1, 2, 13).Merge();

            // Column 3
            var cell3 = worksheet.Cell(3, 1);
            cell3.Value = $"{discharges[0].DischargedDate.Year} Discharges";
            cell3.Style.Font.Bold = true;
            cell3.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range(3, 1, 3, 13).Merge();

            // Column 4: Merged Time column
            var timeCell = worksheet.Range(4, 6, 4, 7);
            timeCell.Merge();
            timeCell.Value = "Time";
            timeCell.Style.Font.Bold = true;
            timeCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Rest of columns
            worksheet.Cell(5, 1).Value = "No.";
            worksheet.Cell(5, 2).Value = "Date Processed";
            worksheet.Cell(5, 3).Value = "Date of Discharge";
            worksheet.Cell(5, 4).Value = "Name of Patient";
            worksheet.Cell(5, 5).Value = "Ward";
            worksheet.Cell(5, 6).Value = "Received HB";
            worksheet.Cell(5, 7).Value = "Issued MSS Faci";
            worksheet.Cell(5, 8).Value = "Duration";
            worksheet.Cell(5, 9).Value = "Class";
            worksheet.Cell(5, 10).Value = "PHIC Category";
            worksheet.Cell(5, 11).Value = "PHIC Used?";
            worksheet.Cell(5, 12).Value = "Remarks if NO";
            worksheet.Cell(5, 13).Value = "MSW";

            int row = 6;
            foreach (var discharge in discharges)
            {
                worksheet.Cell(row, 1).Value = discharge.Id;
                worksheet.Cell(row, 2).Value = discharge.ProcessedDate.ToString();
                worksheet.Cell(row, 3).Value = discharge.DischargedDate.ToString();
                worksheet.Cell(row, 4).Value = $"{discharge.LastName}, {discharge.FirstName} {discharge.MiddleName}";
                worksheet.Cell(row, 5).Value = discharge.Ward;
                worksheet.Cell(row, 6).Value = discharge.ReceivedHB.ToString();
                worksheet.Cell(row, 7).Value = discharge.IssuedMSS.ToString();
                worksheet.Cell(row, 8).Value = discharge.Duration.ToString();
                worksheet.Cell(row, 9).Value = discharge.Class;
                worksheet.Cell(row, 10).Value = discharge.PHICCategory;
                worksheet.Cell(row, 11).Value = discharge.PHICUsed ? "Yes" : "No";
                worksheet.Cell(row, 12).Value = discharge.RemarksIfNo;
                worksheet.Cell(row, 13).Value = discharge.MSW;
                // Add more fields...
                row++;
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                stream.Position = 0;
                return File(stream.ToArray(), 
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                            $"{fileName}.xlsx");
            }

        }

        public async Task<IActionResult> Reports()
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);
            var discharges = await context.Discharges.ToListAsync();
            if (discharges == null || !discharges.Any())
            {
                TempData["ErrorMessage"] = "No discharges found.";
                return RedirectToAction("Index");
            }

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            var viewModel = new DischargeViewModel
            {
                Discharges = discharges,
                Users = users
            };
            return View(viewModel);
        }

        public async Task<IActionResult> ExportReportsToExcel(int userID, string? month)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            // Parse the month input if provided
            bool filterByMonth = DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedMonth);

            var query = context.Discharges.AsQueryable();

            if (userID > 0)
            {
                query = query.Where(d => d.UserID == userID);
            }

            if (filterByMonth)
            {
                query = query.Where(d => d.DischargedDate.Month == parsedMonth.Month && d.DischargedDate.Year == parsedMonth.Year);
            }

            var discharges = await query.ToListAsync();

            if (discharges == null || !discharges.Any())
            {
                TempData["ErrorMessage"] = "No Discharge records found for selected filters.";
                return RedirectToAction("Reports");
            }

            // File name generation
            string mswName = userID > 0 ? discharges.First().MSW : "All MSW";
            string monthLabel = filterByMonth ? parsedMonth.ToString("MMMM_yyyy") : discharges.First().DischargedDate.Year.ToString();
            string fileName = $"Discharge_Reports_{monthLabel}";

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(fileName);

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            // HEADERS
            // COUNTA OF DATE PROCESSED BY MSW
            worksheet.Cell(1, 1).Value = "COUNTA OF DATE PROCESSED BY MSW";
            worksheet.Cell(2, 1).Value = "Date Processed";

            int col = 2;
            foreach (var user in users)
            {
                worksheet.Cell(2, col).Value = user.Username;
                col++;
            }

            worksheet.Cell(2, col).Value = "Grand Total";

            // Prepare data grouped by ProcessedDate
            var groupedDischarges = discharges
                .GroupBy(d => d.ProcessedDate)
                .OrderBy(g => g.Key)
                .ToList();

            int row = 3;
            foreach (var group in groupedDischarges)
            {
                worksheet.Cell(row, 1).Value = group.Key.ToShortDateString();

                int currentCol = 2;
                foreach (var user in users)
                {
                    int count = group.Count(d => d.UserID == user.UserID);
                    worksheet.Cell(row, currentCol).Value = count;
                    currentCol++;
                }

                worksheet.Cell(row, currentCol).Value = group.Count(); // Grand total
                row++;
            }

            // After data rows
            int totalRow = row; 

            worksheet.Cell(totalRow, 1).Value = "Total";

            // For each user column, calculate total discharges across all dates
            int userCol = 2;
            foreach (var user in users)
            {
                int userTotal = discharges.Count(d => d.UserID == user.UserID);
                worksheet.Cell(totalRow, userCol).Value = userTotal;
                userCol++;
            }

            // Grand total: total number of discharge records
            worksheet.Cell(totalRow, userCol).Value = discharges.Count;
            worksheet.Row(totalRow).Style.Font.Bold = true;

            // HEADERS
            // COUNTA Of Class
            worksheet.Cell(totalRow + 2, 1).Value = "Counta of Class";
            worksheet.Cell(totalRow + 3, 1).Value = "Class";

            int classCol = 2;
            foreach (var user in users)
            {
                worksheet.Cell(totalRow + 3, classCol).Value = user.Username;
                classCol++;
            }

            worksheet.Cell(totalRow + 3, classCol).Value = "Grand Total";

            // Prepare data grouped by Class
            var classes = new List<string>  
            {
                "A", "B", "C1", "C2", "C3", "D"
            };
            var groupedClass = discharges
                .Where(d => !string.IsNullOrEmpty(d.Class))
                .GroupBy(d => d.Class)
                .ToDictionary(g => g.Key, g => g.ToList());

            int classRow = totalRow + 4;
            foreach (var cls in classes)
            {
                worksheet.Cell(classRow, 1).Value = cls;

                int currentClassCol = 2;
                foreach (var user in users)
                {
                    int count = groupedClass.ContainsKey(cls)
                        ? groupedClass[cls].Count(d => d.UserID == user.UserID)
                        : 0;

                    worksheet.Cell(classRow, currentClassCol).Value = count;
                    currentClassCol++;
                }

                // Grand total per class
                worksheet.Cell(classRow, currentClassCol).Value = groupedClass.ContainsKey(cls) ? groupedClass[cls].Count : 0;

                classRow++;
            }

            worksheet.Cell(classRow, 1).Value = "Total";
            int totalClassCol = 2;
            foreach (var user in users)
            {
                int totalPerUser = discharges.Count(d => d.UserID == user.UserID && classes.Contains(d.Class));
                worksheet.Cell(classRow, totalClassCol).Value = totalPerUser;
                totalClassCol++;
            }
            worksheet.Cell(classRow, totalClassCol).Value = discharges.Count(d => classes.Contains(d.Class));
            worksheet.Row(classRow).Style.Font.Bold = true;

            // Autofit for better presentation
            worksheet.Columns().AdjustToContents();

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                stream.Position = 0;
                return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"{fileName}.xlsx");
            }

        }

    }
}
