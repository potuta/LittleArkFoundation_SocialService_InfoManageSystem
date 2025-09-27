using DinkToPdf.Contracts;
using DocumentFormat.OpenXml.InkML;
using LittleArkFoundation.Areas.Admin.Data;
using LittleArkFoundation.Areas.Admin.Models;
using LittleArkFoundation.Areas.Admin.Models.Assessments;
using LittleArkFoundation.Areas.Admin.Models.Diagnoses;
using LittleArkFoundation.Areas.Admin.Models.FamilyComposition;
using LittleArkFoundation.Areas.Admin.Models.FamilyHistory;
using LittleArkFoundation.Areas.Admin.Models.Form;
using LittleArkFoundation.Areas.Admin.Models.GeneralAdmission;
using LittleArkFoundation.Areas.Admin.Models.HospitalizationHistory;
using LittleArkFoundation.Areas.Admin.Models.MedicalHistory;
using LittleArkFoundation.Areas.Admin.Models.Medications;
using LittleArkFoundation.Areas.Admin.Models.MentalHealthHistory;
using LittleArkFoundation.Areas.Admin.Models.MSWDClassification;
using LittleArkFoundation.Areas.Admin.Models.Patients;
using LittleArkFoundation.Areas.Admin.Models.ProgressNotes;
using LittleArkFoundation.Areas.Admin.Models.Referrals;
using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using PdfSharp.Drawing;
using System.Data;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LittleArkFoundation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [HasPermission("ManageForm")]
    public class FormController : Controller
    {
        private readonly ConnectionService _connectionService;
        private readonly IWebHostEnvironment _environment;
        private readonly IConverter _pdfConverter;

        public FormController(ConnectionService connectionService, IWebHostEnvironment environment, IConverter pdfConverter)
        {
            _connectionService = connectionService;
            _environment = environment;
            _pdfConverter = pdfConverter;
        }

        public async Task<IActionResult> Index(bool? isActive, int page = 1, int pageSize = 20)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using (var context = new ApplicationDbContext(connectionString))
            {
                bool activeFlag = isActive ?? true;
                ViewBag.isActive = activeFlag;

                var query = context.Patients
                    .Where(u => u.IsActive == activeFlag);

                // Pagination
                var totalCount = await query.CountAsync();
                var patients = await query
                    .OrderBy(p => p.PatientID) 
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var assessments = await context.Assessments
                    .OrderByDescending(a => a.Id)
                    .ThenByDescending(a => a.DateOfInterview)
                    .ThenByDescending(a => a.TimeOfInterview)
                    .ToListAsync();

                var mswdclassification = await context.MSWDClassification
                    .ToListAsync();

                var viewModel = new PatientsViewModel
                {
                    Patients = patients,
                    Assessments = assessments,
                    MSWDClassifications = mswdclassification,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                };

                return View(viewModel);
            }
        }

        public async Task<IActionResult> Search(string searchString, bool? isActive, int page = 1, int pageSize = 20)
        {
            bool activeFlag = isActive ?? true;
            ViewBag.isActive = activeFlag;

            if (string.IsNullOrEmpty(searchString))
            {
                // If no search string, return all patients with the specified active flag
                return RedirectToAction("Index", new { isActive = activeFlag });
            }

            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var searchWords = searchString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var query = context.Patients.Where(u => u.IsActive == activeFlag);

            foreach (var word in searchWords)
            {
                var term = word.Trim();

                query = query.Where(u =>
                    EF.Functions.Like(u.FirstName, $"%{term}%") ||
                    EF.Functions.Like(u.MiddleName, $"%{term}%") ||
                    EF.Functions.Like(u.LastName, $"%{term}%") ||
                    EF.Functions.Like(u.PatientID.ToString(), $"%{term}%"));
            }

            // Pagination
            var totalCount = await query.CountAsync();
            var patients = await query
                .OrderBy(p => p.PatientID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var assessments = await context.Assessments
                    .OrderByDescending(a => a.Id)
                    .ThenByDescending(a => a.DateOfInterview)
                    .ThenByDescending(a => a.TimeOfInterview)
                    .ToListAsync();

            var mswdclassification = await context.MSWDClassification
                .ToListAsync();

            var viewModel = new PatientsViewModel
            {
                Patients = patients,
                Assessments = assessments,
                MSWDClassifications = mswdclassification,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
            };

            return View("Index", viewModel);
        }

        public async Task<IActionResult> ViewHistory(int id)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using (var context = new ApplicationDbContext(connectionString))
            {
                var patient = await context.Patients.FindAsync(id);

                var assessments = await context.Assessments
                    .Where(a => a.PatientID == id)
                    .OrderByDescending(a => a.Id)
                    .ThenByDescending(a => a.DateOfInterview)
                    .ThenByDescending(a => a.TimeOfInterview)
                    .ToListAsync();

                var medicalhistory = await context.MedicalHistory
                    .Where(d => d.PatientID == id)
                    .ToListAsync();

                var users = await context.Users
                    .ToListAsync();

                var viewModel = new PatientsViewModel
                {
                    Patient = patient,
                    Assessments = assessments,
                    MedicalHistory = medicalhistory,
                    Users = users
                };

                ViewBag.UserIDName = User.FindFirst(ClaimTypes.Name);

                return View(viewModel);
            }
        }

        [HasPermission("CreateForm")]
        public async Task<IActionResult> Create()
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using var context = new ApplicationDbContext(connectionString);

            // USERS & SOCIAL WORKER ROLE ID
            var socialWorkerRoleId = await context.Roles
                .Where(r => r.RoleName == "Social Worker")
                .Select(r => r.RoleID)
                .FirstOrDefaultAsync();

            var users = await context.Users
                .Where(u => u.RoleID == socialWorkerRoleId)
                .ToListAsync();

            var viewModel = new FormViewModel()
            {
                Users = users,
                FamilyMembers = new List<FamilyCompositionModel>() { new FamilyCompositionModel() },
                Diagnoses = new List<DiagnosesModel>() { new DiagnosesModel() },
                Medications = new List<MedicationsModel> { new MedicationsModel() },
                HospitalizationHistory = new List<HospitalizationHistoryModel> { new HospitalizationHistoryModel() },
                MentalHealthHistory = new List<MentalHealthHistoryModel> { new MentalHealthHistoryModel() },
                FamilyHistory = new List<FamilyHistoryModel> { new FamilyHistoryModel() },
                ProgressNotes = new List<ProgressNotesModel> { new ProgressNotesModel() }
            };

            return View(viewModel);
        }

        [HasPermission("CreateForm")]
        public async Task<IActionResult> AdmitOPD(int Id)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            // USERS & SOCIAL WORKER ROLE ID
            var socialWorkerRoleId = await context.Roles
                .Where(r => r.RoleName == "Social Worker")
                .Select(r => r.RoleID)
                .FirstOrDefaultAsync();

            var users = await context.Users
                .Where(u => u.RoleID == socialWorkerRoleId)
                .ToListAsync();

            var opd = await context.OPD
                .Where(o => o.Id == Id)
                .FirstOrDefaultAsync();

            var patient = new PatientsModel
            {
                FirstName = opd.FirstName,
                MiddleName = opd.MiddleName,
                LastName = opd.LastName,
            };

            var assessment = new AssessmentsModel
            {
                Age = opd.Age,
                Gender = opd.Gender,
                PermanentAddress = opd.Address,
                MonthlyIncome = opd.MonthlyIncome,
                ContactNo = opd.ContactNo
            };

            var mswdClassification = new MSWDClassificationModel
            {
                SubClassification = opd.Class
            };

            var referral = new ReferralsModel
            {
                ReferralType = opd.SourceOfReferral
            };

            var medicalHistory = new MedicalHistoryModel
            {
                AdmittingDiagnosis = opd.Diagnosis,
            };

            var familyMembers = new List<FamilyCompositionModel>()
            {
                new FamilyCompositionModel
                {
                    Name = $"{opd.MotherFirstName} {opd.MotherMiddleName} {opd.MotherLastName}",
                    Occupation = opd.MotherOccupation,
                    RelationshipToPatient = "Mother"
                },
                new FamilyCompositionModel
                {
                    Name = $"{opd.FatherFirstName} {opd.FatherMiddleName} {opd.FatherLastName}",
                    Occupation = opd.FatherOccupation,
                    RelationshipToPatient = "Father"
                }
            };

            var viewModel = new FormViewModel()
            {
                Users = users,
                FamilyMembers = familyMembers,
                Diagnoses = new List<DiagnosesModel>() { new DiagnosesModel() },
                Medications = new List<MedicationsModel> { new MedicationsModel() },
                HospitalizationHistory = new List<HospitalizationHistoryModel> { new HospitalizationHistoryModel() },
                MentalHealthHistory = new List<MentalHealthHistoryModel> { new MentalHealthHistoryModel() },
                FamilyHistory = new List<FamilyHistoryModel> { new FamilyHistoryModel() },
                ProgressNotes = new List<ProgressNotesModel> { new ProgressNotesModel() },
                Patient = patient,
                MSWDClassification = mswdClassification,
                Referrals = referral,
                MedicalHistory = medicalHistory,
                Assessments = assessment,
                OpdId = Id
            };

            return View("Create", viewModel);
        }

        [HasPermission("CreateForm")]
        public async Task<IActionResult> InterviewGeneral(int Id)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            // USERS & SOCIAL WORKER ROLE ID
            var socialWorkerRoleId = await context.Roles
                .Where(r => r.RoleName == "Social Worker")
                .Select(r => r.RoleID)
                .FirstOrDefaultAsync();

            var users = await context.Users
                .Where(u => u.RoleID == socialWorkerRoleId)
                .ToListAsync();

            var generalAdmission = await context.GeneralAdmission
                .FindAsync(Id);

            var patient = new PatientsModel
            {
                FirstName = generalAdmission.FirstName,
                MiddleName = generalAdmission.MiddleName,
                LastName = generalAdmission.LastName,
            };

            var assessment = new AssessmentsModel
            {
                Age = generalAdmission.Age,
                ContactNo = generalAdmission.ContactNumber,
                Gender = generalAdmission.Gender,
                PermanentAddress = generalAdmission.CompleteAddress,
                MonthlyIncome = generalAdmission.MonthlyIncome,
            };

            var mswdClassification = new MSWDClassificationModel
            {
                SubClassification = generalAdmission.Class
            };

            var referral = new ReferralsModel
            {
                ReferralType = generalAdmission.Referral
            };

            var medicalHistory = new MedicalHistoryModel
            {
                AdmittingDiagnosis = generalAdmission.Diagnosis,
            };

            var familyMembers = new List<FamilyCompositionModel>()
            {
                new FamilyCompositionModel
                {
                    RelationshipToPatient = "Mother",
                    EducationalAttainment = generalAdmission.MotherEducationalAttainment
                },
                new FamilyCompositionModel
                {
                    RelationshipToPatient = "Father",
                    EducationalAttainment = generalAdmission.FatherEducationalAttainment
                }
            };

            var viewModel = new FormViewModel()
            {
                Users = users,
                FamilyMembers = familyMembers,
                Diagnoses = new List<DiagnosesModel>() { new DiagnosesModel() },
                Medications = new List<MedicationsModel> { new MedicationsModel() },
                HospitalizationHistory = new List<HospitalizationHistoryModel> { new HospitalizationHistoryModel() },
                MentalHealthHistory = new List<MentalHealthHistoryModel> { new MentalHealthHistoryModel() },
                FamilyHistory = new List<FamilyHistoryModel> { new FamilyHistoryModel() },
                ProgressNotes = new List<ProgressNotesModel> { new ProgressNotesModel() },
                Patient = patient,
                MSWDClassification = mswdClassification,
                Referrals = referral,
                MedicalHistory = medicalHistory,
                Assessments = assessment,
                GeneralAdmissionId = Id
            };

            return View("Create", viewModel);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission("CreateForm")]
        public async Task<IActionResult> Create(FormViewModel formViewModel)
        {
            try
            {
                string connectionString = _connectionService.GetCurrentConnectionString();

                await using (var context = new ApplicationDbContext(connectionString))
                {
                    if (!ModelState.IsValid)
                    {
                        return View(formViewModel);
                    }

                    // Save Patient first to get the ID, avoids Forein Key constraint
                    await context.Patients.AddAsync(formViewModel.Patient);
                    await context.SaveChangesAsync();

                    var patientID = formViewModel.Patient.PatientID;
                    var assessmentID = await new AssessmentsRepository(connectionString).GenerateID(patientID);
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                    // ASSESSMENTS
                    formViewModel.Assessments.PatientID = patientID;
                    formViewModel.Assessments.AssessmentID = assessmentID;
                    formViewModel.Assessments.UserID = int.Parse(userIdClaim.Value);

                    // REFERRALS
                    formViewModel.Referrals.PatientID = patientID;
                    formViewModel.Referrals.AssessmentID = assessmentID;
                    formViewModel.Referrals.DateOfReferral = formViewModel.Assessments.DateOfInterview.ToDateTime(formViewModel.Assessments.TimeOfInterview);

                    // INFORMANTS
                    formViewModel.Informants.PatientID = patientID;
                    formViewModel.Informants.AssessmentID = assessmentID;
                    formViewModel.Informants.DateOfInformant = formViewModel.Assessments.DateOfInterview.ToDateTime(formViewModel.Assessments.TimeOfInterview);

                    // FAMILY COMPOSITION
                    foreach (var familyMember in formViewModel.FamilyMembers)
                    {
                        familyMember.PatientID = patientID;
                        familyMember.AssessmentID = assessmentID;
                    }

                    // HOUSEHOLD
                    formViewModel.Household.PatientID = patientID;
                    formViewModel.Household.AssessmentID = assessmentID;

                    // MSWD CLASSIFICATION
                    formViewModel.MSWDClassification.PatientID = patientID;
                    formViewModel.MSWDClassification.AssessmentID = assessmentID;

                    // MONTHLY EXPENSES & UTILITIES
                    formViewModel.MonthlyExpenses.PatientID = patientID;
                    formViewModel.MonthlyExpenses.AssessmentID = assessmentID;
                    formViewModel.Utilities.PatientID = patientID;
                    formViewModel.Utilities.AssessmentID = assessmentID;

                    // MEDICAL HISTORY
                    formViewModel.MedicalHistory.PatientID = patientID;
                    formViewModel.MedicalHistory.AssessmentID = assessmentID;

                    // CHILD HEALTH
                    formViewModel.ChildHealth.PatientID = patientID;
                    formViewModel.ChildHealth.AssessmentID = assessmentID;

                    // DIAGNOSES
                    foreach (var diagnosis in formViewModel.Diagnoses)
                    {
                        diagnosis.PatientID = patientID;
                        diagnosis.AssessmentID = assessmentID;
                    }

                    // MEDICATIONS
                    foreach (var medication in formViewModel.Medications)
                    {
                        medication.PatientID = patientID;
                        medication.AssessmentID = assessmentID;
                    }

                    // HOSPITALIZATION HISTORY
                    foreach (var hospitalization in formViewModel.HospitalizationHistory)
                    {
                        hospitalization.PatientID = patientID;
                        hospitalization.AssessmentID = assessmentID;
                    }

                    // MEDICAL SCREENINGS
                    formViewModel.MedicalScreenings.PatientID = patientID;
                    formViewModel.MedicalScreenings.AssessmentID = assessmentID;

                    // PRIMARY CARE DOCTOR
                    formViewModel.PrimaryCareDoctor.PatientID = patientID;
                    formViewModel.PrimaryCareDoctor.AssessmentID = assessmentID;

                    // PRESENTING PROBLEMS
                    formViewModel.PresentingProblems.PatientID = patientID;
                    formViewModel.PresentingProblems.AssessmentID = assessmentID;

                    // RECENT LOSSES
                    formViewModel.RecentLosses.PatientID = patientID;
                    formViewModel.RecentLosses.AssessmentID = assessmentID;

                    // PREGNANCY BIRTH HISTORY
                    formViewModel.PregnancyBirthHistory.PatientID = patientID;
                    formViewModel.PregnancyBirthHistory.AssessmentID = assessmentID;

                    // DEVELOPMENTAL HISTORY
                    formViewModel.DevelopmentalHistory.PatientID = patientID;
                    formViewModel.DevelopmentalHistory.AssessmentID = assessmentID;

                    // MENTAL HEALTH HISTORY
                    foreach (var mentalHealthHistory in formViewModel.MentalHealthHistory)
                    {
                        mentalHealthHistory.PatientID = patientID;
                        mentalHealthHistory.AssessmentID = assessmentID;
                    }

                    // FAMILY HISTORY   
                    foreach (var familyHistory in formViewModel.FamilyHistory)
                    {
                        familyHistory.PatientID = patientID;
                        familyHistory.AssessmentID = assessmentID;
                    }

                    // SAFETY CONCERNS
                    formViewModel.SafetyConcerns.PatientID = patientID;
                    formViewModel.SafetyConcerns.AssessmentID = assessmentID;

                    // CURRENT FUNCTIONING
                    formViewModel.CurrentFunctioning.PatientID = patientID;
                    formViewModel.CurrentFunctioning.AssessmentID = assessmentID;

                    // PARENT CHILD RELATIONSHIP
                    formViewModel.ParentChildRelationship.PatientID = patientID;
                    formViewModel.ParentChildRelationship.AssessmentID = assessmentID;

                    // EDUCATION
                    formViewModel.Education.PatientID = patientID;
                    formViewModel.Education.AssessmentID = assessmentID;

                    // EMPLOYMENT
                    formViewModel.Employment.PatientID = patientID;
                    formViewModel.Employment.AssessmentID = assessmentID;

                    // HOUSING
                    formViewModel.Housing.PatientID = patientID;
                    formViewModel.Housing.AssessmentID = assessmentID;

                    // FOSTER CARE
                    formViewModel.FosterCare.PatientID = patientID;
                    formViewModel.FosterCare.AssessmentID = assessmentID;

                    // ALCOHOL DRUG ASSESSMENT
                    formViewModel.AlcoholDrugAssessment.PatientID = patientID;
                    formViewModel.AlcoholDrugAssessment.AssessmentID = assessmentID;

                    // LEGAL INVOLVEMENT
                    formViewModel.LegalInvolvement.PatientID = patientID;
                    formViewModel.LegalInvolvement.AssessmentID = assessmentID;

                    // HISTORY OF ABUSE
                    formViewModel.HistoryOfAbuse.PatientID = patientID;
                    formViewModel.HistoryOfAbuse.AssessmentID = assessmentID;

                    // HISTORY OF VIOLENCE
                    formViewModel.HistoryOfViolence.PatientID = patientID;
                    formViewModel.HistoryOfViolence.AssessmentID = assessmentID;

                    // STRENGTHS RESOURCES
                    formViewModel.StrengthsResources.PatientID = patientID;
                    formViewModel.StrengthsResources.AssessmentID = assessmentID;

                    // GOALS
                    formViewModel.Goals.PatientID = patientID;
                    formViewModel.Goals.AssessmentID = assessmentID;

                    // PROGRESS NOTES
                    //foreach (var progressNote in formViewModel.ProgressNotes)
                    //{
                    //    progressNote.PatientID = patientID;
                    //    progressNote.AssessmentID = assessmentID;
                    //}
                    formViewModel.ProgressNotes = new List<ProgressNotesModel>{
                        new ProgressNotesModel
                        {
                            PatientID = patientID,
                            AssessmentID = assessmentID
                        }
                    };

                    // GENERAL ADMISSION
                    if (formViewModel.GeneralAdmissionId > 0)
                    {
                        var existingAdmission = await context.GeneralAdmission.FindAsync(formViewModel.GeneralAdmissionId);
                        if (existingAdmission != null)
                        {
                            // Update existing admission
                            existingAdmission.AssessmentID = assessmentID;
                            existingAdmission.PatientID = patientID;
                            existingAdmission.isInterviewed = true;
                        }
                    }
                    else
                    {
                        var educationalAttainment = "N/A";
                        switch (formViewModel.Assessments.EducationLevel)
                        {
                            case "Primary":
                                educationalAttainment = "Elementary Graduate";
                                break;
                            case "Secondary":
                                educationalAttainment = "Senior High School Graduate";
                                break;
                            case "Vocational":
                                educationalAttainment = "Vocational Graduate";
                                break;
                            case "Tertiary":
                                educationalAttainment = "College Graduate";
                                break;
                            case "No Educational Attainment":
                                educationalAttainment = "N/A";
                                break;
                        }

                        var motherEducationalAttainment = formViewModel.FamilyMembers
                            .FirstOrDefault(f => string.Equals(f.RelationshipToPatient, "Mother", StringComparison.OrdinalIgnoreCase))
                            ?.EducationalAttainment ?? "N/A";

                        var fatherEducationalAttainment = formViewModel.FamilyMembers
                            .FirstOrDefault(f => string.Equals(f.RelationshipToPatient, "Father", StringComparison.OrdinalIgnoreCase))
                            ?.EducationalAttainment ?? "N/A";

                        var generalAdmission = new GeneralAdmissionModel
                        {
                            AssessmentID = assessmentID,
                            PatientID = patientID,
                            Date = formViewModel.Assessments.DateOfInterview,
                            isOld = false,
                            HospitalNo = 0, // This will be set later
                            FirstName = formViewModel.Patient.FirstName,
                            MiddleName = formViewModel.Patient.MiddleName,
                            LastName = formViewModel.Patient.LastName,
                            Ward = formViewModel.Assessments.BasicWard,
                            Class = formViewModel.MSWDClassification.SubClassification,
                            Age = formViewModel.Assessments.Age,
                            Gender = formViewModel.Assessments.Gender,
                            Time = formViewModel.Assessments.TimeOfInterview,
                            Diagnosis = formViewModel.MedicalHistory.AdmittingDiagnosis,
                            CompleteAddress = formViewModel.Assessments.PermanentAddress,
                            ContactNumber = formViewModel.Assessments.ContactNo,
                            Referral = formViewModel.Referrals.ReferralType,
                            Occupation = formViewModel.Assessments.Occupation,
                            MonthlyIncome = formViewModel.Assessments.MonthlyIncome.Value,
                            HouseholdSize = formViewModel.Household.HouseholdSize,
                            EducationalAttainment = educationalAttainment,
                            MotherEducationalAttainment = motherEducationalAttainment,
                            FatherEducationalAttainment = fatherEducationalAttainment,
                            isInterviewed = true,
                            MSW = User.FindFirstValue(ClaimTypes.Name), // Assuming the MSW is the user who is logged in
                            UserID = int.Parse(userIdClaim.Value),
                        };

                        await context.GeneralAdmission.AddAsync(generalAdmission);
                    }

                    // OPD
                    if (formViewModel.OpdId > 0)
                    {
                        var opd = await context.OPD.FindAsync(formViewModel.OpdId);
                        if (opd != null)
                        {
                            opd.IsAdmitted = true;
                        }
                    }

                    await context.Assessments.AddAsync(formViewModel.Assessments);
                    await context.SaveChangesAsync();

                    // Update the rest of the form
                    await context.Referrals.AddAsync(formViewModel.Referrals);
                    await context.Informants.AddAsync(formViewModel.Informants);
                    await context.FamilyComposition.AddRangeAsync(formViewModel.FamilyMembers);
                    await context.Households.AddAsync(formViewModel.Household);
                    await context.MSWDClassification.AddAsync(formViewModel.MSWDClassification);
                    await context.MonthlyExpenses.AddAsync(formViewModel.MonthlyExpenses);
                    await context.Utilities.AddAsync(formViewModel.Utilities);
                    await context.MedicalHistory.AddAsync(formViewModel.MedicalHistory);
                    await context.ChildHealth.AddAsync(formViewModel.ChildHealth);
                    await context.Diagnoses.AddRangeAsync(formViewModel.Diagnoses);
                    await context.Medications.AddRangeAsync(formViewModel.Medications);
                    await context.HospitalizationHistory.AddRangeAsync(formViewModel.HospitalizationHistory);
                    await context.MedicalScreenings.AddAsync(formViewModel.MedicalScreenings);
                    await context.PrimaryCareDoctor.AddAsync(formViewModel.PrimaryCareDoctor);
                    await context.PresentingProblems.AddAsync(formViewModel.PresentingProblems);
                    await context.RecentLosses.AddAsync(formViewModel.RecentLosses);
                    await context.PregnancyBirthHistory.AddAsync(formViewModel.PregnancyBirthHistory);
                    await context.DevelopmentalHistory.AddAsync(formViewModel.DevelopmentalHistory);
                    await context.MentalHealthHistory.AddRangeAsync(formViewModel.MentalHealthHistory);
                    await context.FamilyHistory.AddRangeAsync(formViewModel.FamilyHistory);
                    await context.SafetyConcerns.AddAsync(formViewModel.SafetyConcerns);
                    await context.CurrentFunctioning.AddAsync(formViewModel.CurrentFunctioning);
                    await context.ParentChildRelationship.AddAsync(formViewModel.ParentChildRelationship);
                    await context.Education.AddAsync(formViewModel.Education);
                    await context.Employment.AddAsync(formViewModel.Employment);
                    await context.Housing.AddAsync(formViewModel.Housing);
                    await context.FosterCare.AddAsync(formViewModel.FosterCare);
                    await context.AlcoholDrugAssessment.AddAsync(formViewModel.AlcoholDrugAssessment);
                    await context.LegalInvolvement.AddAsync(formViewModel.LegalInvolvement);
                    await context.HistoryOfAbuse.AddAsync(formViewModel.HistoryOfAbuse);
                    await context.HistoryOfViolence.AddAsync(formViewModel.HistoryOfViolence);
                    await context.StrengthsResources.AddAsync(formViewModel.StrengthsResources);
                    await context.Goals.AddAsync(formViewModel.Goals);
                    await context.ProgressNotes.AddRangeAsync(formViewModel.ProgressNotes);
                    //await context.GeneralAdmission.AddAsync(generalAdmission);
                    await context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Successfully created new form";
                    LoggingService.LogInformation($"UserID: {userIdClaim.Value}. Admission Patient creation successful. Created new AssessmentID: {assessmentID} and PatientID: {patientID}");
                    return RedirectToAction("Index");
                }
            }
            catch (SqlException se)
            {
                TempData["ErrorMessage"] = "SQL Error: " + se.Message;
                LoggingService.LogError("SQL Error: " + se);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                LoggingService.LogError("Error: " + ex);
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> ViewForm(int id, int assessmentID)
        {
            // Load the HTML template
            string templatePath = Path.Combine(_environment.WebRootPath, "templates/page1_form_template.html");
            string templatePath2 = Path.Combine(_environment.WebRootPath, "templates/page2_form_template.html");
            string templatePath3 = Path.Combine(_environment.WebRootPath, "templates/page3.2_form_template.html");
            string templatePath4 = Path.Combine(_environment.WebRootPath, "templates/page4_form_template.html");
            string templatePath5 = Path.Combine(_environment.WebRootPath, "templates/page5_form_template.html");
            string templatePath6 = Path.Combine(_environment.WebRootPath, "templates/page6_form_template.html");
            string templatePath7 = Path.Combine(_environment.WebRootPath, "templates/page7_form_template.html");
            string templatePath8 = Path.Combine(_environment.WebRootPath, "templates/page8_form_template.html");
            string templatePath9 = Path.Combine(_environment.WebRootPath, "templates/page9_form_template.html");

            if (!System.IO.File.Exists(templatePath))
            {
                return StatusCode(500, "Form template not found.");
            }

            if (!System.IO.File.Exists(templatePath2))
            {
                return StatusCode(500, "Form template not found.");
            }

            if (!System.IO.File.Exists(templatePath3))
            {
                return StatusCode(500, "Form template not found.");
            }

            if (!System.IO.File.Exists(templatePath4))
            {
                return StatusCode(500, "Form template not found.");
            }

            if (!System.IO.File.Exists(templatePath5))
            {
                return StatusCode(500, "Form template not found.");
            }

            if (!System.IO.File.Exists(templatePath6))
            {
                return StatusCode(500, "Form template not found.");
            }

            if (!System.IO.File.Exists(templatePath7))
            {
                return StatusCode(500, "Form template not found.");
            }

            if (!System.IO.File.Exists(templatePath8))
            {
                return StatusCode(500, "Form template not found.");
            }

            if (!System.IO.File.Exists(templatePath9))
            {
                return StatusCode(500, "Form template not found.");
            }

            string htmlContent = await System.IO.File.ReadAllTextAsync(templatePath);
            htmlContent = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page1(htmlContent, id, assessmentID);

            string htmlContent2 = await System.IO.File.ReadAllTextAsync(templatePath2);
            htmlContent2 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page2(htmlContent2, id, assessmentID);

            string htmlContent3 = await System.IO.File.ReadAllTextAsync(templatePath3);
            htmlContent3 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page3(htmlContent3, id, assessmentID);

            string htmlContent4 = await System.IO.File.ReadAllTextAsync(templatePath4);
            htmlContent4 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page4(htmlContent4, id, assessmentID);

            string htmlContent5 = await System.IO.File.ReadAllTextAsync(templatePath5);
            htmlContent5 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page5(htmlContent5, id, assessmentID);

            string htmlContent6 = await System.IO.File.ReadAllTextAsync(templatePath6);
            htmlContent6 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page6(htmlContent6, id, assessmentID);

            string htmlContent7 = await System.IO.File.ReadAllTextAsync(templatePath7);
            htmlContent7 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page7(htmlContent7, id, assessmentID);

            string htmlContent8 = await System.IO.File.ReadAllTextAsync(templatePath8);
            htmlContent8 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page8(htmlContent8, id, assessmentID);

            string htmlContent9 = await System.IO.File.ReadAllTextAsync(templatePath9);
            var listHtmlContent9 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page9(htmlContent9, id, assessmentID);

            var htmlResults = new List<string>
            {
                htmlContent,
                htmlContent2,
                htmlContent3,
                htmlContent4,
                htmlContent5,
                htmlContent6,
                htmlContent7,
                htmlContent8
            };

            foreach (var html in listHtmlContent9)
            {
                htmlResults.Add(html);
            }

            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);
            var progressNotes = await context.ProgressNotes
                .Where(p => p.PatientID == id && p.AssessmentID == assessmentID)
                .ToListAsync();

            foreach (var progressNote in progressNotes)
            {
                if (progressNote.Attachment != null && progressNote.Attachment.Length > 0)
                {
                    if (progressNote.AttachmentContentType?.StartsWith("image/") == true)
                    {
                        string base64Image = Convert.ToBase64String(progressNote.Attachment);
                        string imageHtml = $"<div style='text-align:center;'><img src='data:{progressNote.AttachmentContentType};base64,{base64Image}' style='max-width:100%; height:auto;' /></div>";
                        htmlResults.Add(imageHtml);
                    }
                    else if (progressNote.AttachmentContentType == "application/pdf")
                    {
                        string base64Pdf = Convert.ToBase64String(progressNote.Attachment);
                        string pdfHtml = $@"
                        <div style='text-align:center;'>
                            <iframe src='data:application/pdf;base64,{base64Pdf}' 
                                    width='100%' height='800px' style='border:none;'></iframe>
                        </div>";
                        htmlResults.Add(pdfHtml);
                    }
                }
            }

            var latestAssessment = await context.Assessments
                .Where(a => a.PatientID == id)
                .OrderByDescending(a => a.Id)
                .ThenByDescending(a => a.DateOfInterview)
                .ThenByDescending(a => a.TimeOfInterview)
                .FirstOrDefaultAsync();

            var currentAssessment = await context.Assessments
                .FirstOrDefaultAsync(a => a.PatientID == id && a.AssessmentID == assessmentID);

            bool isLatestAssessment = latestAssessment != null && currentAssessment != null &&
                                      latestAssessment.AssessmentID == currentAssessment.AssessmentID;

            var patient = await context.Patients
                .FirstOrDefaultAsync(p => p.PatientID == id);

            bool isActive = patient != null && patient.IsActive;

            return View(new HtmlFormViewModel
            {
                Id = id,
                AssessmentID = assessmentID,
                HtmlPages = htmlResults,
                isLatestAssessment = isLatestAssessment,
                isActive = isActive
            });
        }

        [HasPermission("EditForm")]
        public async Task<IActionResult> Edit(int id, int assessmentID)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using var context = new ApplicationDbContext(connectionString);

            // USERS & SOCIAL WORKER ROLE ID
            var socialWorkerRoleId = await context.Roles
                .Where(r => r.RoleName == "Social Worker")
                .Select(r => r.RoleID)
                .FirstOrDefaultAsync();

            var users = await context.Users
                .Where(u => u.RoleID == socialWorkerRoleId)
                .ToListAsync();

            var assessment = await context.Assessments.FirstOrDefaultAsync(a => a.AssessmentID == assessmentID && a.PatientID == id);
            var referral = await context.Referrals.FirstOrDefaultAsync(r => r.AssessmentID == assessmentID && r.PatientID == id);
            var informant = await context.Informants.FirstOrDefaultAsync(i => i.AssessmentID == assessmentID && i.PatientID == id);
            var patient = await context.Patients.FindAsync(id);
            var familymembers = await context.FamilyComposition
                                .Where(f => f.AssessmentID == assessmentID && f.PatientID == id)
                                .ToListAsync();
            var household = await context.Households.FirstOrDefaultAsync(h => h.AssessmentID == assessmentID && h.PatientID == id);
            var mswdClassification = await context.MSWDClassification.FirstOrDefaultAsync(m => m.AssessmentID == assessmentID && m.PatientID == id);
            var monthlyExpenses = await context.MonthlyExpenses.FirstOrDefaultAsync(m => m.AssessmentID == assessmentID && m.PatientID == id);
            var utilities = await context.Utilities.FirstOrDefaultAsync(u => u.AssessmentID == assessmentID && u.PatientID == id);
            var medicalHistory = await context.MedicalHistory.FirstOrDefaultAsync(m => m.AssessmentID == assessmentID && m.PatientID == id);
            var childHealth = await context.ChildHealth.FirstOrDefaultAsync(c => c.AssessmentID == assessmentID && c.PatientID == id);
            var diagnoses = await context.Diagnoses
                                .Where(d => d.AssessmentID == assessmentID && d.PatientID == id)
                                .ToListAsync();
            var medications = await context.Medications
                                .Where(m  => m.AssessmentID == assessmentID && m.PatientID == id)
                                .ToListAsync();
            var hospitalization = await context.HospitalizationHistory
                                .Where(h => h.AssessmentID == assessmentID && h.PatientID == id)
                                .ToListAsync();
            var medicalscreenings = await context.MedicalScreenings.FirstOrDefaultAsync(m => m.AssessmentID == assessmentID && m.PatientID == id);
            var primarycaredoctor = await context.PrimaryCareDoctor.FirstOrDefaultAsync(m => m.AssessmentID == assessmentID && m.PatientID == id);
            var presentingproblems = await context.PresentingProblems.FirstOrDefaultAsync(m => m.AssessmentID == assessmentID && m.PatientID == id);
            var recentlosses = await context.RecentLosses.FirstOrDefaultAsync(m => m.AssessmentID == assessmentID && m.PatientID == id);
            var pregnancybirthhistory = await context.PregnancyBirthHistory.FirstOrDefaultAsync(m => m.AssessmentID == assessmentID && m.PatientID == id);
            var developmentalhistory = await context.DevelopmentalHistory.FirstOrDefaultAsync(m => m.AssessmentID == assessmentID && m.PatientID == id);
            var mentalhealthhistory = await context.MentalHealthHistory
                                .Where(m => m.AssessmentID == assessmentID && m.PatientID == id)
                                .ToListAsync();
            var familyhistory = await context.FamilyHistory
                                .Where(f => f.AssessmentID == assessmentID && f.PatientID == id)
                                .ToListAsync();
            var safetyconcerns = await context.SafetyConcerns.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == id);
            var currentfunctioning = await context.CurrentFunctioning.FirstOrDefaultAsync(c => c.AssessmentID == assessmentID && c.PatientID == id);
            var parentchildrelationship = await context.ParentChildRelationship.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID && p.PatientID == id);
            var education = await context.Education.FirstOrDefaultAsync(e => e.AssessmentID == assessmentID && e.PatientID == id);
            var employment = await context.Employment.FirstOrDefaultAsync(e => e.AssessmentID == assessmentID && e.PatientID == id);
            var housing = await context.Housing.FirstOrDefaultAsync(h => h.AssessmentID == assessmentID && h.PatientID == id);
            var fostercare = await context.FosterCare.FirstOrDefaultAsync(f => f.AssessmentID == assessmentID && f.PatientID == id);
            var alcoholdrugassessment = await context.AlcoholDrugAssessment.FirstOrDefaultAsync(a => a.AssessmentID == assessmentID && a.PatientID == id);
            var legalinvolvement = await context.LegalInvolvement.FirstOrDefaultAsync(l => l.AssessmentID == assessmentID && l.PatientID == id);
            var historyofabuse = await context.HistoryOfAbuse.FirstOrDefaultAsync(h => h.AssessmentID == assessmentID && h.PatientID == id);
            var historyofviolence = await context.HistoryOfViolence.FirstOrDefaultAsync(h => h.AssessmentID == assessmentID && h.PatientID == id);
            var strengthsresources = await context.StrengthsResources.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID && s.PatientID == id);
            var goals = await context.Goals.FirstOrDefaultAsync(g => g.AssessmentID == assessmentID && g.PatientID == id);
            var progressnotes = await context.ProgressNotes
                                .Where(p => p.AssessmentID == assessmentID && p.PatientID == id)
                                .ToListAsync();

            var viewModel = new FormViewModel()
            {
                Users = users,
                Assessments = assessment,
                Referrals = referral,
                Informants = informant,
                Patient = patient,
                FamilyMembers = familymembers,
                Household = household,
                MSWDClassification = mswdClassification,
                MonthlyExpenses = monthlyExpenses,
                Utilities = utilities,
                MedicalHistory = medicalHistory,
                ChildHealth = childHealth,
                Diagnoses = diagnoses,
                Medications = medications,
                HospitalizationHistory = hospitalization,
                MedicalScreenings = medicalscreenings,
                PrimaryCareDoctor = primarycaredoctor,
                PresentingProblems = presentingproblems,
                RecentLosses = recentlosses,
                PregnancyBirthHistory = pregnancybirthhistory,
                DevelopmentalHistory = developmentalhistory,
                MentalHealthHistory = mentalhealthhistory,
                FamilyHistory = familyhistory,
                SafetyConcerns = safetyconcerns,
                CurrentFunctioning = currentfunctioning,
                ParentChildRelationship = parentchildrelationship,
                Education = education,
                Employment = employment,
                Housing = housing,
                FosterCare = fostercare,
                AlcoholDrugAssessment = alcoholdrugassessment,
                LegalInvolvement = legalinvolvement,
                HistoryOfAbuse = historyofabuse,
                HistoryOfViolence = historyofviolence,
                StrengthsResources = strengthsresources,
                Goals = goals,
                ProgressNotes = progressnotes
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission("EditForm")]
        public async Task<IActionResult> Edit(FormViewModel formViewModel)
        {
            try
            {
                string connectionString = _connectionService.GetCurrentConnectionString();

                await using var context = new ApplicationDbContext(connectionString);
                int id = formViewModel.Patient.PatientID;
                int assessmentId = formViewModel.Assessments.AssessmentID;
                var familyMembers = context.FamilyComposition.Where(f => f.AssessmentID == assessmentId && f.PatientID == id);
                var diagnoses = context.Diagnoses.Where(d => d.AssessmentID == assessmentId && d.PatientID == id);
                var medications = context.Medications.Where(m  => m.AssessmentID == assessmentId && m.PatientID == id);
                var hospitalization = context.HospitalizationHistory.Where(h => h.AssessmentID == assessmentId && h.PatientID == id);
                var mentalhealthhistory = context.MentalHealthHistory.Where(m => m.AssessmentID == assessmentId && m.PatientID == id);
                var familyhistory = context.FamilyHistory.Where(f => f.AssessmentID == assessmentId && f.PatientID == id);
                var progressnotes = context.ProgressNotes.Where(p => p.AssessmentID == assessmentId && p.PatientID == id);

                if (familyMembers.Any())
                {
                    context.FamilyComposition.RemoveRange(familyMembers);
                }
                if (diagnoses.Any())
                {
                    context.Diagnoses.RemoveRange(diagnoses);
                }
                if (medications.Any())
                {
                    context.Medications.RemoveRange(medications);
                }
                if (hospitalization.Any())
                {
                    context.HospitalizationHistory.RemoveRange(hospitalization);
                }
                if (mentalhealthhistory.Any())
                {
                    context.MentalHealthHistory.RemoveRange(mentalhealthhistory);
                }
                if (familyhistory.Any())
                {
                    context.FamilyHistory.RemoveRange(familyhistory);
                }
                if (progressnotes.Any())
                {
                    context.ProgressNotes.RemoveRange(progressnotes);
                }

                if (formViewModel.FamilyMembers != null)
                {
                    foreach (var familyMember in formViewModel.FamilyMembers)
                    {
                        familyMember.PatientID = id;
                        familyMember.AssessmentID = assessmentId;
                    }
                    await context.FamilyComposition.AddRangeAsync(formViewModel.FamilyMembers);
                }
                if (formViewModel.Diagnoses != null)
                {
                    foreach (var diagnosis in formViewModel.Diagnoses)
                    {
                        diagnosis.PatientID = id;
                        diagnosis.AssessmentID = assessmentId;
                    }
                    await context.Diagnoses.AddRangeAsync(formViewModel.Diagnoses);
                }
                if (formViewModel.Medications != null)
                {
                    foreach (var medication in formViewModel.Medications)
                    {
                        medication.PatientID = id;
                        medication.AssessmentID = assessmentId;
                    }
                    await context.Medications.AddRangeAsync(formViewModel.Medications);
                }
                if (formViewModel.HospitalizationHistory != null)
                {
                    foreach (var hospitalizationhistory in formViewModel.HospitalizationHistory)
                    {
                        hospitalizationhistory.PatientID = id;
                        hospitalizationhistory.AssessmentID = assessmentId;
                    }
                    await context.HospitalizationHistory.AddRangeAsync(formViewModel.HospitalizationHistory);
                }
                if (formViewModel.MentalHealthHistory != null)
                {
                    foreach (var mentalHealthHistory in formViewModel.MentalHealthHistory)
                    {
                        mentalHealthHistory.PatientID = id;
                        mentalHealthHistory.AssessmentID = assessmentId;
                    }
                    await context.MentalHealthHistory.AddRangeAsync(formViewModel.MentalHealthHistory);
                }
                if (formViewModel.FamilyHistory != null)
                {
                    foreach (var familyHistory in formViewModel.FamilyHistory)
                    {
                        familyHistory.PatientID = id;
                        familyHistory.AssessmentID = assessmentId;
                    }
                    await context.FamilyHistory.AddRangeAsync(formViewModel.FamilyHistory);
                }
                if (formViewModel.ProgressNotes != null)
                {
                    foreach (var progressNote in formViewModel.ProgressNotes)
                    {
                        progressNote.PatientID = id;
                        progressNote.AssessmentID = assessmentId;

                        if (progressNote.RemoveAttachment)
                        {
                            progressNote.Attachment = null;
                            progressNote.AttachmentContentType = null;
                        }
                        else if (progressNote.AttachmentFile != null && progressNote.AttachmentFile.Length > 0)
                        {
                            using (var ms = new MemoryStream())
                            {
                                await progressNote.AttachmentFile.CopyToAsync(ms);
                                progressNote.Attachment = ms.ToArray();
                            }
                            progressNote.AttachmentContentType = progressNote.AttachmentFile.ContentType;
                        }

                        if (progressNote.UserID == 0)
                        {
                            progressNote.UserID = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                        }

                    }

                    await context.ProgressNotes.AddRangeAsync(formViewModel.ProgressNotes);
                }

                // GENERAL ADMISSION
                var generalAdmission = await context.GeneralAdmission
                    .FirstOrDefaultAsync(s => s.AssessmentID == assessmentId && s.PatientID == id);

                if (generalAdmission != null)
                {
                    var educationalAttainment = "N/A";
                    switch (formViewModel.Assessments.EducationLevel)
                    {
                        case "Primary":
                            educationalAttainment = "Elementary Graduate";
                            break;
                        case "Secondary":
                            educationalAttainment = "Senior High School Graduate";
                            break;
                        case "Vocational":
                            educationalAttainment = "Vocational Graduate";
                            break;
                        case "Tertiary":
                            educationalAttainment = "College Graduate";
                            break;
                        case "No Educational Attainment":
                            educationalAttainment = "N/A";
                            break;
                    }

                    var motherEducationalAttainment = formViewModel.FamilyMembers
                        .FirstOrDefault(f => string.Equals(f.RelationshipToPatient, "Mother", StringComparison.OrdinalIgnoreCase))
                        ?.EducationalAttainment ?? "N/A";

                    var fatherEducationalAttainment = formViewModel.FamilyMembers
                        .FirstOrDefault(f => string.Equals(f.RelationshipToPatient, "Father", StringComparison.OrdinalIgnoreCase))
                        ?.EducationalAttainment ?? "N/A";

                    generalAdmission.FirstName = formViewModel.Patient.FirstName;
                    generalAdmission.MiddleName = formViewModel.Patient.MiddleName;
                    generalAdmission.LastName = formViewModel.Patient.LastName;
                    generalAdmission.Ward = formViewModel.Assessments.BasicWard;
                    generalAdmission.Class = formViewModel.MSWDClassification.SubClassification;
                    generalAdmission.Age = formViewModel.Assessments.Age;
                    generalAdmission.Gender = formViewModel.Assessments.Gender;
                    generalAdmission.Time = formViewModel.Assessments.TimeOfInterview;
                    generalAdmission.Diagnosis = formViewModel.MedicalHistory.AdmittingDiagnosis;
                    generalAdmission.CompleteAddress = formViewModel.Assessments.PermanentAddress;
                    generalAdmission.ContactNumber = formViewModel.Assessments.ContactNo;
                    generalAdmission.Referral = formViewModel.Referrals.ReferralType;
                    generalAdmission.Occupation = formViewModel.Assessments.Occupation;
                    generalAdmission.MonthlyIncome = formViewModel.Assessments.MonthlyIncome.Value;
                    generalAdmission.HouseholdSize = formViewModel.Household.HouseholdSize;
                    generalAdmission.EducationalAttainment = educationalAttainment;
                    generalAdmission.MotherEducationalAttainment = motherEducationalAttainment;
                    generalAdmission.FatherEducationalAttainment = fatherEducationalAttainment;
                    // No need to call Update here unless using a detached context
                    // context.GeneralAdmission.Update(generalAdmission);
                }

                // Update Patient first, avoids Forein Key constraint
                context.Patients.Update(formViewModel.Patient);
                await context.SaveChangesAsync();

                context.Assessments.Update(formViewModel.Assessments);
                await context.SaveChangesAsync();

                // Update the rest of the form
                context.Referrals.Update(formViewModel.Referrals);
                context.Informants.Update(formViewModel.Informants);
                context.Households.Update(formViewModel.Household);
                context.MSWDClassification.Update(formViewModel.MSWDClassification);
                context.MonthlyExpenses.Update(formViewModel.MonthlyExpenses);
                context.Utilities.Update(formViewModel.Utilities);
                context.MedicalHistory.Update(formViewModel.MedicalHistory);
                context.ChildHealth.Update(formViewModel.ChildHealth);
                context.MedicalScreenings.Update(formViewModel.MedicalScreenings);
                context.PrimaryCareDoctor.Update(formViewModel.PrimaryCareDoctor);
                context.PresentingProblems.Update(formViewModel.PresentingProblems);
                context.RecentLosses.Update(formViewModel.RecentLosses);
                context.PregnancyBirthHistory.Update(formViewModel.PregnancyBirthHistory);
                context.DevelopmentalHistory.Update(formViewModel.DevelopmentalHistory);
                context.SafetyConcerns.Update(formViewModel.SafetyConcerns);
                context.CurrentFunctioning.Update(formViewModel.CurrentFunctioning);
                context.ParentChildRelationship.Update(formViewModel.ParentChildRelationship);
                context.Education.Update(formViewModel.Education);
                context.Employment.Update(formViewModel.Employment);
                context.Housing.Update(formViewModel.Housing);
                context.FosterCare.Update(formViewModel.FosterCare);
                context.AlcoholDrugAssessment.Update(formViewModel.AlcoholDrugAssessment);
                context.LegalInvolvement.Update(formViewModel.LegalInvolvement);
                context.HistoryOfAbuse.Update(formViewModel.HistoryOfAbuse);
                context.HistoryOfViolence.Update(formViewModel.HistoryOfViolence);
                context.StrengthsResources.Update(formViewModel.StrengthsResources);
                context.Goals.Update(formViewModel.Goals);
                await context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Successfully edited PatientID: {id}";
                LoggingService.LogInformation($"UserID: {User.FindFirst(ClaimTypes.NameIdentifier).Value}. Admission Patient edit successful. Edited AssessmentID: {assessmentId} and PatientID: {id}");
                return RedirectToAction("Index");
            }
            catch (SqlException se)
            {
                TempData["ErrorMessage"] = "SQL Error: " + se.Message;
                LoggingService.LogError("SQL Error: " + se);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                LoggingService.LogError("Error: " + ex);
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadPDF(int id, int assessmentID, bool? isPrint)
        {
            try
            {
                //// Load the HTML template
                string templatePath = Path.Combine(_environment.WebRootPath, "templates/page1_form_template.html");
                string templatePath2 = Path.Combine(_environment.WebRootPath, "templates/page2_form_template.html");
                string templatePath3 = Path.Combine(_environment.WebRootPath, "templates/page3.2_form_template.html");
                string templatePath4 = Path.Combine(_environment.WebRootPath, "templates/page4_form_template.html");
                string templatePath5 = Path.Combine(_environment.WebRootPath, "templates/page5_form_template.html");
                string templatePath6 = Path.Combine(_environment.WebRootPath, "templates/page6_form_template.html");
                string templatePath7 = Path.Combine(_environment.WebRootPath, "templates/page7_form_template.html");
                string templatePath8 = Path.Combine(_environment.WebRootPath, "templates/page8_form_template.html");
                string templatePath9 = Path.Combine(_environment.WebRootPath, "templates/page9_form_template.html");

                if (!System.IO.File.Exists(templatePath))
                {
                    return StatusCode(500, "Form template not found.");
                }

                if (!System.IO.File.Exists(templatePath2))
                {
                    return StatusCode(500, "Form template not found.");
                }

                if (!System.IO.File.Exists(templatePath3))
                {
                    return StatusCode(500, "Form template not found.");
                }

                if (!System.IO.File.Exists(templatePath4))
                {
                    return StatusCode(500, "Form template not found.");
                }

                if (!System.IO.File.Exists(templatePath5))
                {
                    return StatusCode(500, "Form template not found.");
                }

                if (!System.IO.File.Exists(templatePath6))
                {
                    return StatusCode(500, "Form template not found.");
                }

                if (!System.IO.File.Exists(templatePath7))
                {
                    return StatusCode(500, "Form template not found.");
                }

                if (!System.IO.File.Exists(templatePath8))
                {
                    return StatusCode(500, "Form template not found.");
                }

                if (!System.IO.File.Exists(templatePath9))
                {
                    return StatusCode(500, "Form template not found.");
                }

                string htmlContent = await System.IO.File.ReadAllTextAsync(templatePath);
                htmlContent = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page1(htmlContent, id, assessmentID);

                string htmlContent2 = await System.IO.File.ReadAllTextAsync(templatePath2);
                htmlContent2 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page2(htmlContent2, id, assessmentID);

                string htmlContent3 = await System.IO.File.ReadAllTextAsync(templatePath3);
                htmlContent3 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page3(htmlContent3, id, assessmentID);

                string htmlContent4 = await System.IO.File.ReadAllTextAsync(templatePath4);
                htmlContent4 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page4(htmlContent4, id, assessmentID);

                string htmlContent5 = await System.IO.File.ReadAllTextAsync(templatePath5);
                htmlContent5 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page5(htmlContent5, id, assessmentID);

                string htmlContent6 = await System.IO.File.ReadAllTextAsync(templatePath6);
                htmlContent6 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page6(htmlContent6, id, assessmentID);

                string htmlContent7 = await System.IO.File.ReadAllTextAsync(templatePath7);
                htmlContent7 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page7(htmlContent7, id, assessmentID);

                string htmlContent8 = await System.IO.File.ReadAllTextAsync(templatePath8);
                htmlContent8 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page8(htmlContent8, id, assessmentID);

                string htmlContent9 = await System.IO.File.ReadAllTextAsync(templatePath9);
                var listHtmlContent9 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page9(htmlContent9, id, assessmentID);

                var pdf1 = await new PDFService(_pdfConverter).GeneratePdfAsync(htmlContent);
                var pdf2 = await new PDFService(_pdfConverter).GeneratePdfAsync(htmlContent2);
                var pdf3 = await new PDFService(_pdfConverter).GeneratePdfAsync(htmlContent3);
                var pdf4 = await new PDFService(_pdfConverter).GeneratePdfAsync(htmlContent4);
                var pdf5 = await new PDFService(_pdfConverter).GeneratePdfAsync(htmlContent5);
                var pdf6 = await new PDFService(_pdfConverter).GeneratePdfAsync(htmlContent6);
                var pdf7 = await new PDFService(_pdfConverter).GeneratePdfAsync(htmlContent7);
                var pdf8 = await new PDFService(_pdfConverter).GeneratePdfAsync(htmlContent8);

                List<byte[]> pdfList = new List<byte[]>
                {
                    pdf1,
                    pdf2,
                    pdf3,
                    pdf4,
                    pdf5,
                    pdf6,
                    pdf7,
                    pdf8
                };

                foreach (var htmlContentItem in listHtmlContent9)
                {
                    var pdf = await new PDFService(_pdfConverter).GeneratePdfAsync(htmlContentItem);
                    pdfList.Add(pdf);
                }

                string connectionString = _connectionService.GetCurrentConnectionString();
                await using var context = new ApplicationDbContext(connectionString);
                var progressNotes = await context.ProgressNotes
                    .Where(p => p.PatientID == id && p.AssessmentID == assessmentID)
                    .ToListAsync();

                foreach (var progressNote in progressNotes)
                {
                    if (progressNote.Attachment != null && progressNote.Attachment.Length > 0)
                    {
                        if (progressNote.AttachmentContentType != null && progressNote.AttachmentContentType.StartsWith("image/"))
                        {
                            var parts = progressNote.AttachmentContentType.Split('/');
                            var imageFormat = parts.Length > 1 ? parts[1] : "png"; // Default to png if format is not specified
                            var imagePdf = await new PDFService(_pdfConverter).ConvertImageToPdfAsync(progressNote.Attachment, progressNote.Date.ToShortDateString(), progressNote.ProgressNotes, imageFormat);
                            pdfList.Add(imagePdf);
                        }
                        else if (progressNote.AttachmentContentType == "application/pdf")
                        {
                            var imagePdf = await new PDFService(_pdfConverter).ConvertImageToPdfAsync(progressNote.Attachment, progressNote.Date.ToShortDateString(), progressNote.ProgressNotes, "pdf");
                            pdfList.Add(imagePdf);
                        }

                    }
                }

                byte[] mergedPdf = await new PDFService(_pdfConverter).MergePdfsAsync(pdfList);

                if (isPrint == true)
                {
                    return File(mergedPdf, "application/pdf");
                }

                return File(mergedPdf, "application/pdf", $"{id}.pdf");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProgressNoteFile(int id, bool? isDownload)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var progressNote = await context.ProgressNotes.FindAsync(id);
            if (progressNote == null || progressNote.Attachment == null || progressNote.Attachment.Length == 0)
                return NotFound();

            var fileName = $"progress_note_{id}";

            // Optional: Add file extension based on MIME type
            var contentType = progressNote.AttachmentContentType ?? "application/octet-stream";
            var extension = contentType switch
            {
                "application/pdf" => ".pdf",
                "image/jpeg" => ".jpg",
                "image/jpg" => ".jpg",
                "image/png" => ".png",
                _ => ""
            };

            if (isDownload == true)
            {
                return File(progressNote.Attachment, contentType, fileName + extension);
            }
            else
            {
                return File(progressNote.Attachment, contentType);
            }
        }
    }
}
