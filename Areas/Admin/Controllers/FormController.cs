using LittleArkFoundation.Areas.Admin.Models;
using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using DinkToPdf.Contracts;
using LittleArkFoundation.Areas.Admin.Models.Form;
using LittleArkFoundation.Areas.Admin.Data;
using LittleArkFoundation.Areas.Admin.Models.Patients;
using System.Data;
using LittleArkFoundation.Areas.Admin.Models.FamilyComposition;
using LittleArkFoundation.Areas.Admin.Models.Diagnoses;
using LittleArkFoundation.Areas.Admin.Models.Medications;
using LittleArkFoundation.Areas.Admin.Models.HospitalizationHistory;
using LittleArkFoundation.Areas.Admin.Models.MentalHealthHistory;
using LittleArkFoundation.Areas.Admin.Models.FamilyHistory;
using LittleArkFoundation.Areas.Admin.Models.Assessments;
using LittleArkFoundation.Areas.Admin.Models.MedicalHistory;
using System.Security.Claims;
// TODO: Implement logging for forms
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

        public async Task<IActionResult> Index(bool? isActive)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using (var context = new ApplicationDbContext(connectionString))
            {
                bool activeFlag = isActive ?? true;
                ViewBag.isActive = activeFlag;

                var patients = await context.Patients
                    .Where(u => u.IsActive == activeFlag)
                    .ToListAsync();

                var assessments = await context.Assessments
                    .OrderByDescending(a => a.DateOfInterview)
                    .ToListAsync();

                var mswdclassification = await context.MSWDClassification
                    .ToListAsync();

                var viewModel = new PatientsViewModel
                {
                    Patients = patients,
                    Assessments = assessments,
                    MSWDClassifications = mswdclassification
                };

                return View(viewModel);
            }
        }

        public async Task<IActionResult> ViewHistory(int id)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using (var context = new ApplicationDbContext(connectionString))
            {
                var patient = await context.Patients.FindAsync(id);

                var assessments = await context.Assessments
                    .Where(a => a.PatientID == id)
                    .OrderByDescending(a => a.DateOfInterview)
                    .ToListAsync();

                var medicalhistory = await context.MedicalHistory
                    .Where(d => d.PatientID == id)
                    .ToListAsync();

                var viewModel = new PatientsViewModel
                {
                    Patient = patient,
                    Assessments = assessments,
                    MedicalHistory = medicalhistory
                };

                ViewBag.UserIDName = User.FindFirst(ClaimTypes.Name);

                return View(viewModel);
            }
        }

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
                FamilyHistory = new List<FamilyHistoryModel> { new FamilyHistoryModel() }
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FormViewModel formViewModel)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using (var context = new ApplicationDbContext(connectionString))
            {
                if (!ModelState.IsValid)
                {
                    return View(formViewModel);
                }

                var patientID = await new PatientsRepository(connectionString).GenerateID();
                var assessmentID = await new AssessmentsRepository(connectionString).GenerateID();
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

                // PATIENTS
                formViewModel.Patient.PatientID = patientID;

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

                // Save Patient first to get the ID, avoids Forein Key constraint
                await context.Patients.AddAsync(formViewModel.Patient);
                await context.SaveChangesAsync();

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
                await context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Successfully created new form";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> ViewForm(int id, int assessmentID)
        {
            // Load the HTML template
            string templatePath = Path.Combine(_environment.WebRootPath, "templates/page1_form_template.html");
            string templatePath2 = Path.Combine(_environment.WebRootPath, "templates/page2_form_template.html");
            string templatePath3 = Path.Combine(_environment.WebRootPath, "templates/page3_form_template.html");
            string templatePath4 = Path.Combine(_environment.WebRootPath, "templates/page4_form_template.html");
            string templatePath5 = Path.Combine(_environment.WebRootPath, "templates/page5_form_template.html");
            string templatePath6 = Path.Combine(_environment.WebRootPath, "templates/page6_form_template.html");
            string templatePath7 = Path.Combine(_environment.WebRootPath, "templates/page7_form_template.html");
            string templatePath8 = Path.Combine(_environment.WebRootPath, "templates/page8_form_template.html");

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

            // Pass the modified HTML to the view
            ViewBag.FormHtml1 = htmlContent;
            ViewBag.FormHtml2 = htmlContent2;
            ViewBag.FormHtml3 = htmlContent3;
            ViewBag.FormHtml4 = htmlContent4;
            ViewBag.FormHtml5 = htmlContent5;
            ViewBag.FormHtml6 = htmlContent6;
            ViewBag.FormHtml7 = htmlContent7;
            ViewBag.FormHtml8 = htmlContent8;

            ViewBag.Id = id;
            ViewBag.AssessmentID = assessmentID;

            return View();
        }

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

            var assessment = await context.Assessments.FirstOrDefaultAsync(a => a.AssessmentID == assessmentID);
            var referral = await context.Referrals.FirstOrDefaultAsync(r => r.AssessmentID == assessmentID);
            var informant = await context.Informants.FirstOrDefaultAsync(i => i.AssessmentID == assessmentID);
            var patient = await context.Patients.FindAsync(id);
            var familymembers = await context.FamilyComposition
                                .Where(f => f.AssessmentID == assessmentID)
                                .ToListAsync();
            var household = await context.Households.FirstOrDefaultAsync(h => h.AssessmentID == assessmentID);
            var mswdClassification = await context.MSWDClassification.FirstOrDefaultAsync(m => m.AssessmentID == assessmentID);
            var monthlyExpenses = await context.MonthlyExpenses.FirstOrDefaultAsync(m => m.AssessmentID == assessmentID);
            var utilities = await context.Utilities.FirstOrDefaultAsync(u => u.AssessmentID == assessmentID);
            var medicalHistory = await context.MedicalHistory.FirstOrDefaultAsync(m => m.AssessmentID == assessmentID);
            var childHealth = await context.ChildHealth.FirstOrDefaultAsync(c => c.AssessmentID == assessmentID);
            var diagnoses = await context.Diagnoses
                                .Where(d => d.AssessmentID == assessmentID)
                                .ToListAsync();
            var medications = await context.Medications
                                .Where(m  => m.AssessmentID == assessmentID)
                                .ToListAsync();
            var hospitalization = await context.HospitalizationHistory
                                .Where(h => h.AssessmentID == assessmentID)
                                .ToListAsync();
            var medicalscreenings = await context.MedicalScreenings.FirstOrDefaultAsync(m => m.AssessmentID == assessmentID);
            var primarycaredoctor = await context.PrimaryCareDoctor.FirstOrDefaultAsync(m => m.AssessmentID == assessmentID);
            var presentingproblems = await context.PresentingProblems.FirstOrDefaultAsync(m => m.AssessmentID == assessmentID);
            var recentlosses = await context.RecentLosses.FirstOrDefaultAsync(m => m.AssessmentID == assessmentID);
            var pregnancybirthhistory = await context.PregnancyBirthHistory.FirstOrDefaultAsync(m => m.AssessmentID == assessmentID);
            var developmentalhistory = await context.DevelopmentalHistory.FirstOrDefaultAsync(m => m.AssessmentID == assessmentID);
            var mentalhealthhistory = await context.MentalHealthHistory
                                .Where(m => m.AssessmentID == assessmentID)
                                .ToListAsync();
            var familyhistory = await context.FamilyHistory
                                .Where(f => f.AssessmentID == assessmentID)
                                .ToListAsync();
            var safetyconcerns = await context.SafetyConcerns.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID);
            var currentfunctioning = await context.CurrentFunctioning.FirstOrDefaultAsync(c => c.AssessmentID == assessmentID);
            var parentchildrelationship = await context.ParentChildRelationship.FirstOrDefaultAsync(p => p.AssessmentID == assessmentID);
            var education = await context.Education.FirstOrDefaultAsync(e => e.AssessmentID == assessmentID);
            var employment = await context.Employment.FirstOrDefaultAsync(e => e.AssessmentID == assessmentID);
            var housing = await context.Housing.FirstOrDefaultAsync(h => h.AssessmentID == assessmentID);
            var fostercare = await context.FosterCare.FirstOrDefaultAsync(f => f.AssessmentID == assessmentID);
            var alcoholdrugassessment = await context.AlcoholDrugAssessment.FirstOrDefaultAsync(a => a.AssessmentID == assessmentID);
            var legalinvolvement = await context.LegalInvolvement.FirstOrDefaultAsync(l => l.AssessmentID == assessmentID);
            var historyofabuse = await context.HistoryOfAbuse.FirstOrDefaultAsync(h => h.AssessmentID == assessmentID);
            var historyofviolence = await context.HistoryOfViolence.FirstOrDefaultAsync(h => h.AssessmentID == assessmentID);
            var strengthsresources = await context.StrengthsResources.FirstOrDefaultAsync(s => s.AssessmentID == assessmentID);
            var goals = await context.Goals.FirstOrDefaultAsync(g => g.AssessmentID == assessmentID);

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
                Goals = goals
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FormViewModel formViewModel)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using var context = new ApplicationDbContext(connectionString);
            int id = formViewModel.Patient.PatientID;
            int assessmentId = formViewModel.Assessments.AssessmentID;
            var familyMembers = context.FamilyComposition.Where(f => f.AssessmentID == assessmentId);
            var diagnoses = context.Diagnoses.Where(d => d.AssessmentID == assessmentId);
            var medications = context.Medications.Where(m  => m.AssessmentID == assessmentId);
            var hospitalization = context.HospitalizationHistory.Where(h => h.AssessmentID == assessmentId);
            var mentalhealthhistory = context.MentalHealthHistory.Where(m => m.AssessmentID == assessmentId);
            var familyhistory = context.FamilyHistory.Where(f => f.AssessmentID == assessmentId);

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
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DownloadPDF(int id, int assessmentID)
        {
            try
            {
                //// Load the HTML template
                string templatePath = Path.Combine(_environment.WebRootPath, "templates/page1_form_template.html");
                string templatePath2 = Path.Combine(_environment.WebRootPath, "templates/page2_form_template.html");
                string templatePath3 = Path.Combine(_environment.WebRootPath, "templates/page3_form_template.html");
                string templatePath4 = Path.Combine(_environment.WebRootPath, "templates/page4_form_template.html");
                string templatePath5 = Path.Combine(_environment.WebRootPath, "templates/page5_form_template.html");
                string templatePath6 = Path.Combine(_environment.WebRootPath, "templates/page6_form_template.html");
                string templatePath7 = Path.Combine(_environment.WebRootPath, "templates/page7_form_template.html");
                string templatePath8 = Path.Combine(_environment.WebRootPath, "templates/page8_form_template.html");

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

                //byte[] pdfBytes = _pdfConverter.Convert(pdfDocument);
                byte[] mergedPdf = await new PDFService(_pdfConverter).MergePdfsAsync(pdfList);
                return File(mergedPdf, "application/pdf", $"{id}.pdf");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex.Message);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

    }
}
