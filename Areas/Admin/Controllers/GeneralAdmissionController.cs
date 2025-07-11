using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using LittleArkFoundation.Areas.Admin.Data;
using LittleArkFoundation.Areas.Admin.Models.Discharges;
using LittleArkFoundation.Areas.Admin.Models.GeneralAdmission;
using LittleArkFoundation.Areas.Admin.Models.OPD;
using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace LittleArkFoundation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [HasPermission("ManageForm")]
    public class GeneralAdmissionController : Controller
    {
        private readonly ConnectionService _connectionService;
        public GeneralAdmissionController(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<IActionResult> Index(string? sortToggle)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            string sortToggleValue = sortToggle ?? "All";
            ViewBag.sortToggle = sortToggleValue;

            var generalAdmissions = new List<GeneralAdmissionModel>();

            if (sortToggleValue == "All")
            {
                generalAdmissions = await context.GeneralAdmission.ToListAsync();
            }
            else if (sortToggleValue == "Interviewed")
            {
                generalAdmissions = await context.GeneralAdmission.Where(patient => patient.isInterviewed).ToListAsync();
            }
            else if (sortToggleValue == "Not Interviewed")
            {
                generalAdmissions = await context.GeneralAdmission.Where(patient => !patient.isInterviewed).ToListAsync();
            }

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            var viewModel = new GeneralAdmissionViewModel
            {
                Users = users,
                GeneralAdmissions = generalAdmissions
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Search(string searchString, string? sortToggle)
        {
            string sortToggleValue = sortToggle ?? "All";
            ViewBag.sortToggle = sortToggleValue;

            if (string.IsNullOrEmpty(searchString))
            {
                // If no search string, return all patients with the specified active flag
                return RedirectToAction("Index", new { sortToggle = sortToggleValue });
            }

            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var searchWords = searchString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var query = context.GeneralAdmission.AsQueryable();

            foreach (var word in searchWords)
            {
                var term = word.Trim();

                query = query.Where(u =>
                    EF.Functions.Like(u.FirstName, $"%{term}%") ||
                    EF.Functions.Like(u.MiddleName, $"%{term}%") ||
                    EF.Functions.Like(u.LastName, $"%{term}%") ||
                    EF.Functions.Like(u.Id.ToString(), $"%{term}%"));
            }

            var generalAdmissions = await query.ToListAsync();

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            var viewModel = new GeneralAdmissionViewModel
            {
                Users = users,
                GeneralAdmissions = generalAdmissions
            };

            return View("Index", viewModel);
        }

        public async Task<IActionResult> SortBy(string sortByUserID, string? sortByMonth, string? sortToggle)
        {
            string sortToggleValue = sortToggle ?? "All";
            ViewBag.sortToggle = sortToggleValue;

            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var query = context.GeneralAdmission.AsQueryable();

            if (!string.IsNullOrEmpty(sortByUserID))
            {
                query = query.Where(patient => patient.UserID == int.Parse(sortByUserID));
                var user = await context.Users.FindAsync(int.Parse(sortByUserID));
                ViewBag.sortBy = user.Username;
                ViewBag.sortByUserID = user.UserID.ToString();
            }

            if (sortToggleValue == "Interviewed")
            {
                query = query.Where(patient => patient.isInterviewed);
            }
            else if (sortToggleValue == "Not Interviewed")
            {
                query = query.Where(patient => !patient.isInterviewed);
            }

            if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime month))
            {
                query = query.Where(patient => patient.Date.Month == month.Month && patient.Date.Year == month.Year);
                ViewBag.sortByMonth = month.ToString("yyyy-MM");
            }

            var generalAdmissions = await query.ToListAsync();

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            var viewModel = new GeneralAdmissionViewModel
            {
                Users = users,
                GeneralAdmissions = generalAdmissions
            };

            return View("Index", viewModel);
        }

        public async Task<IActionResult> SortbyReports(string sortByUserID, string? sortByMonth, string? viewName = "Index")
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var query = context.GeneralAdmission.AsQueryable();

            if (!string.IsNullOrEmpty(sortByUserID))
            {
                query = query.Where(d => d.UserID == int.Parse(sortByUserID));
                var user = await context.Users.FindAsync(int.Parse(sortByUserID));
                ViewBag.sortBy = user.Username;
                ViewBag.sortByUserID = user.UserID.ToString();
            }

            if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime month))
            {
                query = query.Where(d => d.Date.Month == month.Month && d.Date.Year == month.Year);
                ViewBag.sortByMonth = month.ToString("yyyy-MM");
            }

            var generalAdmissions = await query.ToListAsync();

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            var viewModel = new GeneralAdmissionViewModel
            {
                GeneralAdmissions = generalAdmissions,
                Users = users
            };

            return View(viewName, viewModel);
        }

        public async Task<IActionResult> Edit(int id)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var generalAdmission = await context.GeneralAdmission.FindAsync(id);

            var viewModel = new GeneralAdmissionViewModel
            {
                GeneralAdmission = generalAdmission,
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GeneralAdmissionViewModel viewModel)
        {
            try
            {
                string connectionString = _connectionService.GetCurrentConnectionString();
                await using var context = new ApplicationDbContext(connectionString);

                context.GeneralAdmission.Update(viewModel.GeneralAdmission);
                await context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Successfully edited General Admission Id: {viewModel.GeneralAdmission.Id}";
                LoggingService.LogInformation($"General Admission Patient edit successful. Edited Id: {viewModel.GeneralAdmission.Id}. Edited by UserID: {User.FindFirst(ClaimTypes.NameIdentifier).Value}, DateTime: {DateTime.Now}");
                return RedirectToAction("Index");
            }
            catch (SqlException se)
            {
                TempData["ErrorMessage"] = "SQL Error: " + se.Message;
                LoggingService.LogError("SQL Error: " + se.Message);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                LoggingService.LogError("Error: " + ex.Message);
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Create()
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);
            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();
            var viewModel = new GeneralAdmissionViewModel
            {
                Users = users,
                GeneralAdmission = new GeneralAdmissionModel()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GeneralAdmissionViewModel viewModel)
        {
            try
            {
                string connectionString = _connectionService.GetCurrentConnectionString();
                await using var context = new ApplicationDbContext(connectionString);

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await context.Users.FindAsync(int.Parse(userIdClaim));

                viewModel.GeneralAdmission.MSW = user?.Username ?? "N/A";
                viewModel.GeneralAdmission.UserID = user?.UserID ?? 0;

                await context.GeneralAdmission.AddAsync(viewModel.GeneralAdmission);
                await context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Successfully created General Admission record.";
                LoggingService.LogInformation($"General Admission Patient creation successful. Created Id: {viewModel.GeneralAdmission.Id}. Created by UserID: {userIdClaim}, DateTime: {DateTime.Now}");
                return RedirectToAction("Index");
            }
            catch (SqlException se)
            {
                TempData["ErrorMessage"] = "SQL Error: " + se.Message;
                LoggingService.LogError("SQL Error: " + se.Message);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                LoggingService.LogError("Error: " + ex.Message);
            }
            return View(viewModel);
        }

        public async Task<IActionResult> ExportLogsheetToExcel(int userID, string? month)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            // Parse the month input if provided
            bool filterByMonth = DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedMonth);

            var query = context.GeneralAdmission.AsQueryable();

            if (userID > 0)
            {
                query = query.Where(p => p.UserID == userID);
            }

            if (filterByMonth)
            {
                query = query.Where(p => p.Date.Month == parsedMonth.Month && p.Date.Year == parsedMonth.Year);
            }

            var generalAdmissions = await query.ToListAsync();

            if (generalAdmissions == null || !generalAdmissions.Any())
            {
                TempData["ErrorMessage"] = "No data found for the specified criteria.";
                return RedirectToAction("Index");
            }

            // File name generation
            string mswName = userID > 0 ? generalAdmissions.First().MSW : "All MSW";
            string monthLabel = filterByMonth ? parsedMonth.ToString("MMMM_yyyy") : generalAdmissions.First().Date.Year.ToString();
            string fileName = $"GA_Logsheet_{monthLabel}_{mswName}";

            // Sanitize sheet name (for Excel)
            string safeSheetName = Regex.Replace(fileName, @"[\[\]\*\?/\\:]", "_");
            if (safeSheetName.Length > 31)
                safeSheetName = safeSheetName.Substring(0, 31);

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(safeSheetName);

            // HEADERS
            var headers = new[]
            {
                "Date", "No", "Old/New", "Hosp No.", "Name of Patient",
                "Ward", "Class", "Age", "Gender", "Time", "Diagnosis",
                "Complete Address", "Origin", "Contact No.", "Referral",
                "Occupation", "Stat's Occupation", "Income Range", "Monthly Income",
                "Eco. Stat", "HH Size", "M. Stat", "PWD", "Patient", "Father", "Mother",
                "Yes", "No", "Dwell", "Light", "Water", "Fuel", "PHIC", "MSW",
            };

            // Column 1
            var cell1 = worksheet.Cell(1, 1);
            cell1.Value = mswName;
            cell1.Style.Font.Bold = true;
            cell1.Style.Font.FontSize = 14;
            cell1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range(1, 1, 1, headers.Count()).Merge(); // Merge across desired columns

            // Column 2
            var cell2 = worksheet.Cell(2, 1);
            cell2.Value = "General Admission";
            cell2.Style.Font.Bold = true;
            cell2.Style.Font.FontSize = 12;
            cell2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range(2, 1, 2, headers.Count()).Merge();

            // Column 3
            var cell3 = worksheet.Cell(3, 1);
            cell3.Value = $"{monthLabel} General Admission";
            cell3.Style.Font.Bold = true;
            cell3.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range(3, 1, 3, headers.Count()).Merge();

            // Rest of columns
            int headerRow = 4;
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(headerRow, i + 1).Value = headers[i];
                worksheet.Cell(headerRow, i + 1).Style.Font.Bold = true;
                worksheet.Cell(headerRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Populate data
            int dataRow = headerRow + 1;
            foreach (var admission in generalAdmissions)
            {
                worksheet.Cell(dataRow, 1).Value = admission.Date.ToShortDateString();
                worksheet.Cell(dataRow, 2).Value = admission.Id;
                worksheet.Cell(dataRow, 3).Value = admission.isOld ? "Old" : "New";
                worksheet.Cell(dataRow, 4).Value = admission.HospitalNo;
                worksheet.Cell(dataRow, 5).Value = $"{admission.LastName}, {admission.FirstName} {admission.MiddleName}";
                worksheet.Cell(dataRow, 6).Value = admission.Ward;
                worksheet.Cell(dataRow, 7).Value = admission.Class;
                worksheet.Cell(dataRow, 8).Value = admission.Age;
                worksheet.Cell(dataRow, 9).Value = admission.Gender;
                worksheet.Cell(dataRow, 10).Value = admission.Time.ToString();
                worksheet.Cell(dataRow, 11).Value = admission.Diagnosis;
                worksheet.Cell(dataRow, 12).Value = admission.CompleteAddress;
                worksheet.Cell(dataRow, 13).Value = admission.Origin;
                worksheet.Cell(dataRow, 14).Value = admission.ContactNumber;
                worksheet.Cell(dataRow, 15).Value = admission.Referral;
                worksheet.Cell(dataRow, 16).Value = admission.Occupation;
                worksheet.Cell(dataRow, 17).Value = admission.StatsOccupation;
                worksheet.Cell(dataRow, 18).Value = admission.IncomeRange;
                worksheet.Cell(dataRow, 19).Value = admission.MonthlyIncome;
                worksheet.Cell(dataRow, 20).Value = admission.EconomicStatus;
                worksheet.Cell(dataRow, 21).Value = admission.HouseholdSize;
                worksheet.Cell(dataRow, 22).Value = admission.MaritalStatus;
                worksheet.Cell(dataRow, 23).Value = admission.isPWD ? "Yes" : "No";
                worksheet.Cell(dataRow, 24).Value = admission.EducationalAttainment;
                worksheet.Cell(dataRow, 25).Value = admission.FatherEducationalAttainment;
                worksheet.Cell(dataRow, 26).Value = admission.MotherEducationalAttainment;
                worksheet.Cell(dataRow, 27).Value = admission.isInterviewed ? "✓" : "-";
                worksheet.Cell(dataRow, 28).Value = admission.isInterviewed ? "-" : "✓";
                worksheet.Cell(dataRow, 29).Value = admission.DwellingType;
                worksheet.Cell(dataRow, 30).Value = admission.LightSource;
                worksheet.Cell(dataRow, 31).Value = admission.WaterSource;
                worksheet.Cell(dataRow, 32).Value = admission.FuelSource;
                worksheet.Cell(dataRow, 33).Value = admission.PHIC;
                worksheet.Cell(dataRow, 34).Value = admission.MSW;

                dataRow++;
            }

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

        public async Task<IActionResult> Reports()
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var generalAdmissions = await context.GeneralAdmission.ToListAsync();
            if (generalAdmissions == null || !generalAdmissions.Any())
            {
                TempData["ErrorMessage"] = "No General Admission records found.";
                return RedirectToAction("Index");
            }

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            var viewModel = new GeneralAdmissionViewModel
            {
                Users = users,
                GeneralAdmissions = generalAdmissions
            };

            return View(viewModel);
        }
    }
}
