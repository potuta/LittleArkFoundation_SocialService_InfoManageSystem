﻿using LittleArkFoundation.Areas.Admin.Models;
using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using DinkToPdf;
using System.Runtime.InteropServices;
using DinkToPdf.Contracts;
using HtmlAgilityPack;
using LittleArkFoundation.Areas.Admin.Models.Form;
using LittleArkFoundation.Areas.Admin.Data;
using LittleArkFoundation.Areas.Admin.Models.Patients;

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

            using (var context = new ApplicationDbContext(connectionString))
            {
                if (!ModelState.IsValid)
                {
                    return View(formViewModel);
                }

                // PATIENTS
                formViewModel.Patient.PatientID = await new PatientsRepository(connectionString).GenerateID();

                await context.Patients.AddAsync(formViewModel.Patient);
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
                var patient = await context.Patients.FindAsync(id);

                if (patient == null)
                {
                    return NotFound();
                }

                // Load the HTML template
                string templatePath = Path.Combine(_environment.WebRootPath, "templates/page1_form_template.html");

                if (!System.IO.File.Exists(templatePath))
                {
                    return StatusCode(500, "Form template not found.");
                }

                string htmlContent = await System.IO.File.ReadAllTextAsync(templatePath);

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);

                // Replace placeholders with user data
                //htmlContent = htmlContent.Replace("{FullName}", response.FullName)
                //                         .Replace("{Email}", response.Email)
                //                         .Replace("{Address}", response.Address)
                //                         .Replace("{Message}", response.Message);

                string date = string.Empty;
                var dateofinterview = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Dateofinterview']");
                if (dateofinterview != null)
                {
                    dateofinterview.InnerHtml = date;
                }

                var sexmalecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Sexmalecheckbox']");
                if (sexmalecheckbox != null)
                {
                    string existingStyle = sexmalecheckbox.GetAttributeValue("style", "");
                    sexmalecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }

                // PATIENTS
                var patientlastname = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Patientsurname']");
                if (patientlastname != null)
                {
                    patientlastname.InnerHtml = patient.LastName;
                }

                htmlContent = htmlDoc.DocumentNode.OuterHtml;

                // Pass the modified HTML to the view
                TempData["FormHtml"] = htmlContent;
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

                var patient = await context.Patients.FindAsync(id);

                if (patient == null)
                {
                    return NotFound();
                }

                //// Load the HTML template
                string templatePath = Path.Combine(_environment.WebRootPath, "templates/page1_form_template.html");

                if (!System.IO.File.Exists(templatePath))
                {
                    return StatusCode(500, "Form template not found.");
                }

                string htmlContent = await System.IO.File.ReadAllTextAsync(templatePath);

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);

                // Replace placeholders with user data
                //htmlContent = htmlContent.Replace("{FullName}", response.FullName)
                //                         .Replace("{Email}", response.Email)
                //                         .Replace("{Address}", response.Address)
                //                         .Replace("{Message}", response.Message);

                string date = string.Empty;
                var dateofinterview = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Dateofinterview']");
                if (dateofinterview != null)
                {
                    dateofinterview.InnerHtml = date;
                }

                var sexmalecheckbox = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Sexmalecheckbox']");
                if (sexmalecheckbox != null)
                {
                    string existingStyle = sexmalecheckbox.GetAttributeValue("style", "");
                    sexmalecheckbox.SetAttributeValue("style", existingStyle + "; background-color: black;");
                }

                // PATIENTS
                var patientlastname = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='Patientsurname']");
                if (patientlastname != null)
                {
                    patientlastname.InnerHtml = patient.LastName;
                }

                htmlContent = htmlDoc.DocumentNode.OuterHtml;

                string imagePath = Path.Combine(_environment.WebRootPath, "resources", "NCH-Logo.png");
                byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
                string base64String = Convert.ToBase64String(imageBytes);
                htmlContent = htmlContent.Replace("/resources/NCH-Logo.png", $"data:image/png;base64,{base64String}");

                var pdfDocument = new HtmlToPdfDocument()
                {
                    GlobalSettings = new GlobalSettings
                    {
                        ColorMode = ColorMode.Color,
                        Orientation = Orientation.Portrait,
                        PaperSize = PaperKind.A4,
                        Margins = new MarginSettings { Top = 0, Bottom = 0, Left = 0, Right = 0 },
                        DocumentTitle = "Generated PDF",
                        DPI = 300
                    },
                    Objects =
                    {
                        new ObjectSettings
                        {
                            HtmlContent = htmlContent,
                            WebSettings = 
                            {
                                DefaultEncoding = "utf-8",
                                LoadImages = true,
                                PrintMediaType = true
                            },
                            UseExternalLinks = true,
                            LoadSettings =
                            {
                                ZoomFactor = 2
                            }
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
