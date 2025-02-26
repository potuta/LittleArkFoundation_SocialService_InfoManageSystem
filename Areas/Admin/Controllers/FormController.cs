using LittleArkFoundation.Areas.Admin.Models;
using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using DinkToPdf.Contracts;
using HtmlAgilityPack;
using LittleArkFoundation.Areas.Admin.Models.Form;
using LittleArkFoundation.Areas.Admin.Data;
using LittleArkFoundation.Areas.Admin.Models.Patients;
using System.Data;

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

        public async Task<IActionResult> Index(string dbType)
        {
            string connectionString = _connectionService.GetConnectionString(dbType);
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

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string dbType, FormViewModel formViewModel)
        {
            string connectionString = _connectionService.GetConnectionString(dbType);

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

                await context.Assessments.AddAsync(formViewModel.Assessments);
                await context.Referrals.AddAsync(formViewModel.Referrals);
                await context.Informants.AddAsync(formViewModel.Informants);
                await context.Patients.AddAsync(formViewModel.Patient);
                await context.SaveChangesAsync();

                TempData["CreateSuccess"] = "Successfully created new form";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> ViewForm(string dbType, int id)
        {
            // Load the HTML template
            string templatePath = Path.Combine(_environment.WebRootPath, "templates/page1_form_template.html");

            if (!System.IO.File.Exists(templatePath))
            {
                return StatusCode(500, "Form template not found.");
            }

            string htmlContent = await System.IO.File.ReadAllTextAsync(templatePath);
            htmlContent = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page1(htmlContent, dbType, id);

            // Pass the modified HTML to the view
            TempData["FormHtml"] = htmlContent;
            ViewBag.Id = id;

            return View();
        }

        // TODO: Implement edit form
        public async Task<IActionResult> Edit()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DownloadPDF(string dbType, int id)
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
                htmlContent = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page1(htmlContent, dbType, id);

                string htmlContent2 = await System.IO.File.ReadAllTextAsync(templatePath2);
                htmlContent2 = await new HtmlTemplateService(_environment, _connectionService).ModifyHtmlTemplateAsync_Page2(htmlContent2, dbType, id);

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
