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

        public async Task<IActionResult> Index()
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using (var context = new ApplicationDbContext(connectionString))
            {
                var patients = await context.Patients.ToListAsync();

                var viewModel = new PatientsViewModel
                {
                    Patients = patients
                };

                return View(viewModel);
            }
        }

        public async Task<IActionResult> Create()
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using var context = new ApplicationDbContext(connectionString);

            var viewModel = new FormViewModel()
            {
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

                // ASSESSMENTS
                formViewModel.Assessments.PatientID = patientID;

                // REFERRALS
                formViewModel.Referrals.PatientID = patientID;
                formViewModel.Referrals.DateOfReferral = formViewModel.Assessments.DateOfInterview.ToDateTime(formViewModel.Assessments.TimeOfInterview);

                // INFORMANTS
                formViewModel.Informants.PatientID = patientID;
                formViewModel.Informants.DateOfInformant = formViewModel.Assessments.DateOfInterview.ToDateTime(formViewModel.Assessments.TimeOfInterview);

                // PATIENTS
                formViewModel.Patient.PatientID = patientID;

                // FAMILY COMPOSITION
                foreach (var familyMember in formViewModel.FamilyMembers)
                {
                    familyMember.PatientID = patientID;
                }

                // HOUSEHOLD
                formViewModel.Household.PatientID = patientID;

                // MSWD CLASSIFICATION
                formViewModel.MSWDClassification.PatientID = patientID;

                // MONTHLY EXPENSES & UTILITIES
                formViewModel.MonthlyExpenses.PatientID = patientID;
                formViewModel.Utilities.PatientID = patientID;

                // MEDICAL HISTORY
                formViewModel.MedicalHistory.PatientID = patientID;

                // CHILD HEALTH
                formViewModel.ChildHealth.PatientID = patientID;

                // DIAGNOSES
                foreach (var diagnosis in formViewModel.Diagnoses)
                {
                    diagnosis.PatientID = patientID;
                }

                // MEDICATIONS
                foreach (var medication in formViewModel.Medications)
                {
                    medication.PatientID = patientID;
                }

                // HOSPITALIZATION HISTORY
                foreach (var hospitalization in formViewModel.HospitalizationHistory)
                {
                    hospitalization.PatientID = patientID;
                }

                // MEDICAL SCREENINGS
                formViewModel.MedicalScreenings.PatientID = patientID;

                // PRIMARY CARE DOCTOR
                formViewModel.PrimaryCareDoctor.PatientID = patientID;

                // PRESENTING PROBLEMS
                formViewModel.PresentingProblems.PatientID = patientID;

                // RECENT LOSSES
                formViewModel.RecentLosses.PatientID = patientID;

                // PREGNANCY BIRTH HISTORY
                formViewModel.PregnancyBirthHistory.PatientID = patientID;

                // DEVELOPMENTAL HISTORY
                formViewModel.DevelopmentalHistory.PatientID = patientID;

                // MENTAL HEALTH HISTORY
                foreach (var mentalHealthHistory in formViewModel.MentalHealthHistory)
                {
                    mentalHealthHistory.PatientID = patientID;
                }

                // FAMILY HISTORY   
                foreach (var familyHistory in formViewModel.FamilyHistory)
                {
                    familyHistory.PatientID = patientID;
                }

                // SAFETY CONCERNS
                formViewModel.SafetyConcerns.PatientID = patientID;

                // Save Patient first to get the ID, avoids Forein Key constraint
                await context.Patients.AddAsync(formViewModel.Patient);
                await context.SaveChangesAsync();

                // Update the rest of the form
                await context.Assessments.AddAsync(formViewModel.Assessments);
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
                await context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Successfully created new form";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> ViewForm(int id)
        {
            // Load the HTML template
            string templatePath = Path.Combine(_environment.WebRootPath, "templates/page1_form_template.html");
            string templatePath2 = Path.Combine(_environment.WebRootPath, "templates/page2_form_template.html");
            string templatePath3 = Path.Combine(_environment.WebRootPath, "templates/page3_form_template.html");
            string templatePath4 = Path.Combine(_environment.WebRootPath, "templates/page4_form_template.html");

            if (!System.IO.File.Exists(templatePath))
            {
                return StatusCode(500, "Form template not found.");
            }

            string htmlContent = await System.IO.File.ReadAllTextAsync(templatePath);
            htmlContent = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page1(htmlContent, id);

            string htmlContent2 = await System.IO.File.ReadAllTextAsync(templatePath2);
            htmlContent2 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page2(htmlContent2, id);

            string htmlContent3 = await System.IO.File.ReadAllTextAsync(templatePath3);
            htmlContent3 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page3(htmlContent3, id);

            string htmlContent4 = await System.IO.File.ReadAllTextAsync(templatePath4);
            htmlContent4 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page4(htmlContent4, id);

            // Pass the modified HTML to the view
            ViewBag.FormHtml1 = htmlContent;
            ViewBag.FormHtml2 = htmlContent2;
            ViewBag.FormHtml3 = htmlContent3;
            ViewBag.FormHtml4 = htmlContent4;

            ViewBag.Id = id;

            return View();
        }

        public async Task<IActionResult> Edit(int id)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using var context = new ApplicationDbContext(connectionString);

            var assessment = await context.Assessments.FirstOrDefaultAsync(a => a.PatientID == id);
            var referral = await context.Referrals.FirstOrDefaultAsync(r => r.PatientID == id);
            var informant = await context.Informants.FirstOrDefaultAsync(i => i.PatientID == id);
            var patient = await context.Patients.FindAsync(id);
            var familymembers = await context.FamilyComposition
                                .Where(f => f.PatientID == id)
                                .ToListAsync();
            var household = await context.Households.FirstOrDefaultAsync(h => h.PatientID == id);
            var mswdClassification = await context.MSWDClassification.FirstOrDefaultAsync(m => m.PatientID == id);
            var monthlyExpenses = await context.MonthlyExpenses.FirstOrDefaultAsync(m => m.PatientID == id);
            var utilities = await context.Utilities.FirstOrDefaultAsync(u => u.PatientID == id);
            var medicalHistory = await context.MedicalHistory.FirstOrDefaultAsync(m => m.PatientID == id);
            var childHealth = await context.ChildHealth.FirstOrDefaultAsync(c => c.PatientID == id);
            var diagnoses = await context.Diagnoses
                                .Where(d => d.PatientID == id)
                                .ToListAsync();
            var medications = await context.Medications
                                .Where(m  => m.PatientID == id)
                                .ToListAsync();
            var hospitalization = await context.HospitalizationHistory
                                .Where(h => h.PatientID == id)
                                .ToListAsync();
            var medicalscreenings = await context.MedicalScreenings.FirstOrDefaultAsync(m => m.PatientID == id);
            var primarycaredoctor = await context.PrimaryCareDoctor.FirstOrDefaultAsync(m => m.PatientID == id);
            var presentingproblems = await context.PresentingProblems.FirstOrDefaultAsync(m => m.PatientID == id);
            var recentlosses = await context.RecentLosses.FirstOrDefaultAsync(m => m.PatientID == id);
            var pregnancybirthhistory = await context.PregnancyBirthHistory.FirstOrDefaultAsync(m => m.PatientID == id);
            var developmentalhistory = await context.DevelopmentalHistory.FirstOrDefaultAsync(m => m.PatientID == id);
            var mentalhealthhistory = await context.MentalHealthHistory
                                .Where(m => m.PatientID == id)
                                .ToListAsync();
            var familyhistory = await context.FamilyHistory
                                .Where(f => f.PatientID == id)
                                .ToListAsync();
            var safetyconcerns = await context.SafetyConcerns.FirstOrDefaultAsync(s => s.PatientID == id);

            var viewModel = new FormViewModel()
            {
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
                SafetyConcerns = safetyconcerns
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
            var familyMembers = context.FamilyComposition.Where(f => f.PatientID == id);
            var diagnoses = context.Diagnoses.Where(d => d.PatientID == id);
            var medications = context.Medications.Where(m  => m.PatientID == id);
            var hospitalization = context.HospitalizationHistory.Where(h => h.PatientID == id);
            var mentalhealthhistory = context.MentalHealthHistory.Where(m => m.PatientID == id);
            var familyhistory = context.FamilyHistory.Where(f => f.PatientID == id);

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
                }
                await context.FamilyComposition.AddRangeAsync(formViewModel.FamilyMembers);
            }
            if (formViewModel.Diagnoses != null)
            {
                foreach (var diagnosis in formViewModel.Diagnoses)
                {
                    diagnosis.PatientID = id;
                }
                await context.Diagnoses.AddRangeAsync(formViewModel.Diagnoses);
            }
            if (formViewModel.Medications != null)
            {
                foreach (var medication in formViewModel.Medications)
                {
                    medication.PatientID = id;
                }
                await context.Medications.AddRangeAsync(formViewModel.Medications);
            }
            if (formViewModel.HospitalizationHistory != null)
            {
                foreach (var hospitalizationhistory in formViewModel.HospitalizationHistory)
                {
                    hospitalizationhistory.PatientID = id;
                }
                await context.HospitalizationHistory.AddRangeAsync(formViewModel.HospitalizationHistory);
            }
            if (formViewModel.MentalHealthHistory != null)
            {
                foreach (var mentalHealthHistory in formViewModel.MentalHealthHistory)
                {
                    mentalHealthHistory.PatientID = id;
                }
                await context.MentalHealthHistory.AddRangeAsync(formViewModel.MentalHealthHistory);
            }
            if (formViewModel.FamilyHistory != null)
            {
                foreach (var familyHistory in formViewModel.FamilyHistory)
                {
                    familyHistory.PatientID = id;
                }
                await context.FamilyHistory.AddRangeAsync(formViewModel.FamilyHistory);
            }

            // Update Patient first, avoids Forein Key constraint
            context.Patients.Update(formViewModel.Patient);
            await context.SaveChangesAsync();

            // Update the rest of the form
            context.Assessments.Update(formViewModel.Assessments);
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
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Successfully edited PatientID: {id}";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DownloadPDF(int id)
        {
            try
            {
                //// Load the HTML template
                string templatePath = Path.Combine(_environment.WebRootPath, "templates/page1_form_template.html");
                string templatePath2 = Path.Combine(_environment.WebRootPath, "templates/page2_form_template.html");
                string templatePath3 = Path.Combine(_environment.WebRootPath, "templates/page3_form_template.html");
                string templatePath4 = Path.Combine(_environment.WebRootPath, "templates/page4_form_template.html");

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

                string htmlContent = await System.IO.File.ReadAllTextAsync(templatePath);
                htmlContent = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page1(htmlContent, id);

                string htmlContent2 = await System.IO.File.ReadAllTextAsync(templatePath2);
                htmlContent2 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page2(htmlContent2, id);

                string htmlContent3 = await System.IO.File.ReadAllTextAsync(templatePath3);
                htmlContent3 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page3(htmlContent3, id);

                string htmlContent4 = await System.IO.File.ReadAllTextAsync(templatePath4);
                htmlContent4 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page4(htmlContent4, id);

                var pdf1 = await new PDFService(_pdfConverter).GeneratePdfAsync(htmlContent);
                var pdf2 = await new PDFService(_pdfConverter).GeneratePdfAsync(htmlContent2);
                var pdf3 = await new PDFService(_pdfConverter).GeneratePdfAsync(htmlContent3);
                var pdf4 = await new PDFService(_pdfConverter).GeneratePdfAsync(htmlContent4);

                List<byte[]> pdfList = new List<byte[]>
                {
                    pdf1,
                    pdf2,
                    pdf3,
                    pdf4
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
