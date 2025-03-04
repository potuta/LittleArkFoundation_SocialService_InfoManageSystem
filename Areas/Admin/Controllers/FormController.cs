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
                FamilyMembers = new List<FamilyCompositionModel>() { new FamilyCompositionModel() }
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

                await context.Assessments.AddAsync(formViewModel.Assessments);
                await context.Referrals.AddAsync(formViewModel.Referrals);
                await context.Informants.AddAsync(formViewModel.Informants);
                await context.Patients.AddAsync(formViewModel.Patient);
                await context.FamilyComposition.AddRangeAsync(formViewModel.FamilyMembers);
                await context.Households.AddAsync(formViewModel.Household);
                await context.MSWDClassification.AddAsync(formViewModel.MSWDClassification);
                await context.SaveChangesAsync();

                TempData["CreateSuccess"] = "Successfully created new form";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> ViewForm(int id)
        {
            // Load the HTML template
            string templatePath = Path.Combine(_environment.WebRootPath, "templates/page1_form_template.html");

            if (!System.IO.File.Exists(templatePath))
            {
                return StatusCode(500, "Form template not found.");
            }

            string htmlContent = await System.IO.File.ReadAllTextAsync(templatePath);
            htmlContent = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page1(htmlContent, id);

            // Pass the modified HTML to the view
            TempData["FormHtml"] = htmlContent;
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

            var viewModel = new FormViewModel()
            {
                Assessments = assessment,
                Referrals = referral,
                Informants = informant,
                Patient = patient,
                FamilyMembers = familymembers,
                Household = household,
                MSWDClassification = mswdClassification
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

            if (familyMembers.Any())
            {
                context.FamilyComposition.RemoveRange(familyMembers);
            }

            if (formViewModel.FamilyMembers != null)
            {
                foreach (var familyMember in formViewModel.FamilyMembers)
                {
                    familyMember.PatientID = id;
                }
                await context.FamilyComposition.AddRangeAsync(formViewModel.FamilyMembers);
            }

            context.Assessments.Update(formViewModel.Assessments);
            context.Referrals.Update(formViewModel.Referrals);
            context.Informants.Update(formViewModel.Informants);
            context.Patients.Update(formViewModel.Patient);
            context.Households.Update(formViewModel.Household);
            context.MSWDClassification.Update(formViewModel.MSWDClassification);

            TempData["EditSuccess"] = $"Successfully edited PatientID: {id}";
            await context.SaveChangesAsync();
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
                string templatePath2 = Path.Combine(_environment.WebRootPath, "templates/sample_form_template.html");

                if (!System.IO.File.Exists(templatePath))
                {
                    return StatusCode(500, "Form template not found.");
                }

                if (!System.IO.File.Exists(templatePath2))
                {
                    return StatusCode(500, "Form template not found.");
                }

                string htmlContent = await System.IO.File.ReadAllTextAsync(templatePath);
                htmlContent = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page1(htmlContent, id);

                string htmlContent2 = await System.IO.File.ReadAllTextAsync(templatePath2);
                htmlContent2 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page2(htmlContent2, id);

                var pdf1 = await new PDFService(_pdfConverter).GeneratePdfAsync(htmlContent);
                var pdf2 = await new PDFService(_pdfConverter).GeneratePdfAsync(htmlContent2);

                List<byte[]> pdfList = new List<byte[]>
                {
                    pdf1,
                    pdf2
                };

                //byte[] pdfBytes = _pdfConverter.Convert(pdfDocument);
                byte[] mergedPdf = await new PDFService(_pdfConverter).MergePdfsAsync(pdfList);
                return File(mergedPdf, "application/pdf", "UserForm.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PDF Generation Error: {ex.Message}");
                return StatusCode(500, "An error occurred while generating the PDF.");
            }
        }

    }
}
