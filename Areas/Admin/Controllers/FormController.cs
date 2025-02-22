using LittleArkFoundation.Areas.Admin.Models;
using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using DinkToPdf;
using System.Runtime.InteropServices;
using DinkToPdf.Contracts;

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
            using (var context = new ApplicationDbContext(connectionString))
            {
                var responses = await context.FormResponses.ToListAsync();

                var viewModel = new FormViewModel
                {
                    FormResponses = responses
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
        public async Task<IActionResult> Create(string dbType, FormResponsesModel form)
        {
            string connectionString = _connectionService.GetConnectionString(dbType);

            using (var context = new ApplicationDbContext(connectionString))
            {
                if (!ModelState.IsValid)
                {
                    return View(form);
                }

                await context.FormResponses.AddAsync(form);
                await context.SaveChangesAsync();

                TempData["CreateSuccess"] = "Successfully created new form";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> ViewForm(string dbType, int id)
        {
            string connectionString = _connectionService.GetConnectionString(dbType);

            using (var context = new ApplicationDbContext(connectionString))
            {
                var response = await context.FormResponses.FindAsync(id);

                if (response == null)
                {
                    return NotFound();
                }

                // Load the HTML template
                string templatePath = Path.Combine(_environment.WebRootPath, "templates/sample_form_template.html");

                if (!System.IO.File.Exists(templatePath))
                {
                    return StatusCode(500, "Form template not found.");
                }

                string htmlContent = await System.IO.File.ReadAllTextAsync(templatePath);

                // Replace placeholders with user data
                htmlContent = htmlContent.Replace("{FullName}", response.FullName)
                                         .Replace("{Email}", response.Email)
                                         .Replace("{Address}", response.Address)
                                         .Replace("{Message}", response.Message);

                // Pass the modified HTML to the view
                ViewData["FormHtml"] = htmlContent;
                ViewBag.Id = id;
            }

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
                string connectionString = _connectionService.GetConnectionString(dbType);

                using var context = new ApplicationDbContext(connectionString);
                var response = await context.FormResponses.FindAsync(id);

                if (response == null)
                {
                    return NotFound();
                }

                // Load the HTML template
                string templatePath = Path.Combine(_environment.WebRootPath, "templates/sample_form_template.html");
                string htmlContent = await System.IO.File.ReadAllTextAsync(templatePath);

                // Replace placeholders with actual data
                htmlContent = htmlContent.Replace("{FullName}", response.FullName)
                                         .Replace("{Email}", response.Email)
                                         .Replace("{Address}", response.Address)
                                         .Replace("{Message}", response.Message);

                var pdfDocument = new HtmlToPdfDocument()
                {
                    GlobalSettings = new GlobalSettings
                    {
                        ColorMode = ColorMode.Color,
                        Orientation = Orientation.Portrait,
                        PaperSize = PaperKind.A4,
                        Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 },
                        DocumentTitle = "Generated PDF"
                    },
                    Objects =
                    {
                        new ObjectSettings
                        {
                            HtmlContent = htmlContent,
                            WebSettings = { DefaultEncoding = "utf-8" }
                        }
                    }
                };

                // Use the injected singleton converter
                byte[] pdfBytes = _pdfConverter.Convert(pdfDocument);

                return File(pdfBytes, "application/pdf", "UserForm.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PDF Generation Error: {ex.Message}");
                return StatusCode(500, "An error occurred while generating the PDF.");
            }
        }

    }
}
