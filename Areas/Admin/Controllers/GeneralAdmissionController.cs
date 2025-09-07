using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using LittleArkFoundation.Areas.Admin.Data;
using LittleArkFoundation.Areas.Admin.Models.Discharges;
using LittleArkFoundation.Areas.Admin.Models.GeneralAdmission;
using LittleArkFoundation.Areas.Admin.Models.OPD;
using LittleArkFoundation.Areas.Admin.Services.Reports;
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

        public async Task<IActionResult> Index(string? sortToggle, string? sortByMonth, int page = 1, int pageSize = 20)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            string sortToggleValue = sortToggle ?? "All";
            ViewBag.sortToggle = sortToggleValue;

            var query = context.GeneralAdmission.AsQueryable();
            
            if (sortToggleValue == "Interviewed")
            {
                query = context.GeneralAdmission.Where(patient => patient.isInterviewed);
            }
            else if (sortToggleValue == "Not Interviewed")
            {
                query = context.GeneralAdmission.Where(patient => !patient.isInterviewed);
            }

            if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime month))
            {
                query = query.Where(opd => opd.Date.Month == month.Month && opd.Date.Year == month.Year);
                ViewBag.sortByMonth = month.ToString("yyyy-MM");
            }

            // Pagination
            var totalCount = await query.CountAsync();
            var generalAdmissions = await query
                .OrderByDescending(g => g.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            //var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            //var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();
            var users = await context.Users.ToListAsync();

            var viewModel = new GeneralAdmissionViewModel
            {
                Users = users,
                GeneralAdmissions = generalAdmissions,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Search(string searchString, string? sortToggle, string? sortByMonth, int page = 1, int pageSize = 20)
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

            if (sortToggleValue == "Interviewed")
            {
                query = context.GeneralAdmission.Where(patient => patient.isInterviewed);
            }
            else if (sortToggleValue == "Not Interviewed")
            {
                query = context.GeneralAdmission.Where(patient => !patient.isInterviewed);
            }

            if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime month))
            {
                query = query.Where(opd => opd.Date.Month == month.Month && opd.Date.Year == month.Year);
                ViewBag.sortByMonth = month.ToString("yyyy-MM");
            }

            foreach (var word in searchWords)
            {
                var term = word.Trim();

                query = query.Where(u =>
                    EF.Functions.Like(u.FirstName, $"%{term}%") ||
                    EF.Functions.Like(u.MiddleName, $"%{term}%") ||
                    EF.Functions.Like(u.LastName, $"%{term}%") ||
                    EF.Functions.Like(u.Id.ToString(), $"%{term}%"));
            }

            // Pagination
            var totalCount = await query.CountAsync();
            var generalAdmissions = await query
                .OrderByDescending(g => g.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            //var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            //var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();
            var users = await context.Users.ToListAsync();

            var viewModel = new GeneralAdmissionViewModel
            {
                Users = users,
                GeneralAdmissions = generalAdmissions,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
            };

            return View("Index", viewModel);
        }

        public async Task<IActionResult> SortBy(string sortByUserID, string? sortByMonth, string? sortToggle, int page = 1, int pageSize = 20)
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

            // Pagination
            var totalCount = await query.CountAsync();
            var generalAdmissions = await query
                .OrderByDescending(g => g.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            //var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            //var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();
            var users = await context.Users.ToListAsync();

            var viewModel = new GeneralAdmissionViewModel
            {
                Users = users,
                GeneralAdmissions = generalAdmissions,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
            };

            return View("Index", viewModel);
        }

        public async Task<IActionResult> SortByReports(string sortByUserID, string? sortToggle, string? sortByMonth, string? viewName = "Index")
        {
            string sortToggleValue = sortToggle ?? "General";
            ViewBag.sortToggle = sortToggleValue;

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

            //var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            //var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();
            var users = await context.Users.ToListAsync();

            var viewModel = new GeneralAdmissionViewModel
            {
                GeneralAdmissions = generalAdmissions,
                Users = users
            };

            return View(viewName, viewModel);
        }

        public async Task<IActionResult> SortByStatistics(string sortByUserID, string? sortByMonth, string? viewName = "Index")
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var query = context.GeneralAdmission.AsQueryable();
            var progressNotesQuery = context.ProgressNotes.AsQueryable();

            if (!string.IsNullOrEmpty(sortByUserID))
            {
                query = query.Where(d => d.UserID == int.Parse(sortByUserID));
                progressNotesQuery = progressNotesQuery.Where(p => p.UserID == int.Parse(sortByUserID));
                var user = await context.Users.FindAsync(int.Parse(sortByUserID));
                ViewBag.sortBy = user.Username;
                ViewBag.sortByUserID = user.UserID.ToString();
            }

            if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime month))
            {
                query = query.Where(d => d.Date.Month == month.Month && d.Date.Year == month.Year);
                progressNotesQuery = progressNotesQuery.Where(p => p.Date.Month == month.Month && p.Date.Year == month.Year);
                ViewBag.sortByMonth = month.ToString("yyyy-MM");
            }

            var generalAdmissions = await query.ToListAsync();
            var progressNotes = await progressNotesQuery.ToListAsync();

            //var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            //var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();
            var users = await context.Users.ToListAsync();

            var viewModel = new GeneralAdmissionViewModel
            {
                GeneralAdmissions = generalAdmissions,
                ProgressNotes = progressNotes,
                Users = users
            };

            return View(viewName, viewModel);
        }

        [HasPermission("EditForm")]
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
        [HasPermission("EditForm")]
        public async Task<IActionResult> Edit(GeneralAdmissionViewModel viewModel)
        {
            try
            {
                string connectionString = _connectionService.GetCurrentConnectionString();
                await using var context = new ApplicationDbContext(connectionString);

                context.GeneralAdmission.Update(viewModel.GeneralAdmission);
                await context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Successfully edited General Admission Id: {viewModel.GeneralAdmission.Id}";
                LoggingService.LogInformation($"UserID: {User.FindFirst(ClaimTypes.NameIdentifier).Value}. General Admission Patient edit successful. Edited Id: {viewModel.GeneralAdmission.Id}");
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

        [HasPermission("CreateForm")]
        public async Task<IActionResult> Create()
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);
            //var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            //var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();
            var users = await context.Users.ToListAsync();
            var viewModel = new GeneralAdmissionViewModel
            {
                Users = users,
                GeneralAdmission = new GeneralAdmissionModel()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission("CreateForm")]
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
                LoggingService.LogInformation($"UserID: {userIdClaim}. General Admission Patient creation successful. Created Id: {viewModel.GeneralAdmission.Id}");
                return RedirectToAction("Index");
            }
            catch (SqlException se)
            {
                TempData["ErrorMessage"] = "SQL Error: " + se.Message;
                LoggingService.LogError("SQL Error: " + se);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                LoggingService.LogError("Error: " + ex);
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

            ExcelReportStyler.ApplyWorksheetDesign(worksheet, new List<int> { 1, 2, 3 }, new List<int> { headerRow }, new List<int> { dataRow }, dataRow, User.FindFirst(ClaimTypes.Name).Value, false, true);

            // Autofit for better presentation
            worksheet.Columns().AdjustToContents();

            await using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                stream.Position = 0;
                return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"{fileName}.xlsx");
            }
        }

        public async Task<IActionResult> Reports(string? sortToggle, string? sortByMonth)
        {
            string sortToggleValue = sortToggle ?? "General";
            ViewBag.sortToggle = sortToggleValue;

            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var query = context.GeneralAdmission.AsQueryable();

            if (query == null || !query.Any())
            {
                TempData["ErrorMessage"] = "No General Admission records found.";
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime month))
            {
                query = query.Where(patient => patient.Date.Month == month.Month && patient.Date.Year == month.Year);
                ViewBag.sortByMonth = month.ToString("yyyy-MM");
            }

            var generalAdmissions = await query.ToListAsync();

            //var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            //var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();
            var users = await context.Users.ToListAsync();

            var viewModel = new GeneralAdmissionViewModel
            {
                Users = users,
                GeneralAdmissions = generalAdmissions
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ExportReportsToExcel(int userID, string? month, string? sortToggle)
        {
            if (sortToggle == "General")
            {
                return RedirectToAction("ExportGeneralReportsToExcel", new { userID, month });
            }
            else if (sortToggle == "Patient Interviewed")
            {
                return RedirectToAction("ExportPatientInterviewedToExcel", new {userID, month });
            }
            else if (sortToggle == "Total Interviewed")
            {
                return RedirectToAction("ExportTotalInterviewedToExcel", new { userID, month });
            }
            else if (sortToggle == "PHIC")
            {
                return RedirectToAction("ExportPHICToExcel", new { userID, month });
            }
            else if (sortToggle == "Referral/Old/New/PWD")
            {
                return RedirectToAction("ExportReferralOldNewPWDToExcel", new { userID, month });
            }

            TempData["ErrorMessage"] = $"Download failed. No reports was found for {sortToggle}";
            return RedirectToAction("Reports");
        }

        public async Task<IActionResult> ExportGeneralReportsToExcel(int userID, string? month)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            // Parse the month input if provided
            bool filterByMonth = DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedMonth);

            var query = context.GeneralAdmission.AsQueryable();

            if (userID > 0)
            {
                query = query.Where(g => g.UserID == userID);
            }

            if (filterByMonth)
            {
                query = query.Where(g => g.Date.Month == parsedMonth.Month && g.Date.Year == parsedMonth.Year);
            }

            var generalAdmissions = await query.ToListAsync();

            if (generalAdmissions == null || !generalAdmissions.Any())
            {
                TempData["ErrorMessage"] = "No General Admissions records found for selected filters.";
                return RedirectToAction("Reports");
            }

            // File name generation
            string mswName = userID > 0 ? generalAdmissions.First().MSW : "All";
            string monthLabel = filterByMonth ? parsedMonth.ToString("MMMM_yyyy") : generalAdmissions.First().Date.Year.ToString();
            string fileName = $"GA_Reports_{monthLabel}";

            // Sanitize file name (for download)
            string safeFileName = Regex.Replace(fileName, @"[^\w\-]", "_");

            // Sanitize sheet name (for Excel)
            string safeSheetName = Regex.Replace(fileName, @"[\[\]\*\?/\\:]", "_");
            if (safeSheetName.Length > 31)
                safeSheetName = safeSheetName.Substring(0, 31);

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(safeSheetName);

            //var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            //var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();
            var users = await context.Users.ToListAsync();

            // HEADERS
            // COUNT OF DATE PROCESSED BY MSW
            worksheet.Cell(4, 1).Value = "COUNT OF DATE";
            worksheet.Cell(5, 1).Value = "Date";

            int dateColIndex = 2;
            worksheet.Cell(5, dateColIndex).Value = "Grand Total";

            // Prepare data grouped by ProcessedDate
            var groupedOPD = generalAdmissions
                .GroupBy(d => d.Date)
                .OrderBy(g => g.Key)
                .ToList();

            int dateRowIndex = 6;
            foreach (var group in groupedOPD)
            {
                worksheet.Cell(dateRowIndex, 1).Value = group.Key.ToShortDateString();

                int colIndex = 2;

                worksheet.Cell(dateRowIndex, colIndex).Value = group.Count(); // Grand Total
                dateRowIndex++;
            }

            int totalDateRowIndex = dateRowIndex;
            worksheet.Cell(totalDateRowIndex, 1).Value = "Total";

            int totalDateColIndex = 2;

            worksheet.Cell(totalDateRowIndex, totalDateColIndex).Value = groupedOPD.Sum(g => g.Count()); // Grand Total
            worksheet.Row(totalDateRowIndex).Style.Font.Bold = true;

            // HEADERS
            // COUNT OF CLASS 
            int classRowIndex = totalDateRowIndex + 2;

            worksheet.Cell(classRowIndex, 1).Value = "COUNT OF CLASS";
            worksheet.Cell(classRowIndex + 1, 1).Value = "Class";

            int classColIndex = 2;
            foreach (var user in users)
            {
                worksheet.Cell(classRowIndex + 1, classColIndex).Value = user.Username;
                classColIndex++;
            }

            worksheet.Cell(classRowIndex + 1, classColIndex).Value = "Grand Total";

            // Prepare data grouped by Class
            var groupedClass = generalAdmissions
                .GroupBy(d => d.Class)
                .OrderBy(g => g.Key)
                .ToList();

            int classDataRowIndex = classRowIndex + 2;
            foreach (var group in groupedClass)
            {
                worksheet.Cell(classDataRowIndex, 1).Value = group.Key;
                int colIndex = 2;
                foreach (var user in users)
                {
                    var count = group.Count(d => d.UserID == user.UserID);
                    worksheet.Cell(classDataRowIndex, colIndex).Value = count;
                    colIndex++;
                }
                worksheet.Cell(classDataRowIndex, colIndex).Value = group.Count(); // Grand Total
                classDataRowIndex++;
            }

            int totalClassRowIndex = classDataRowIndex;
            worksheet.Cell(totalClassRowIndex, 1).Value = "Total";

            int totalClassColIndex = 2;
            foreach (var user in users)
            {
                var totalCount = groupedClass.Sum(g => g.Count(d => d.UserID == user.UserID));
                worksheet.Cell(totalClassRowIndex, totalClassColIndex).Value = totalCount;
                totalClassColIndex++;
            }

            worksheet.Cell(totalClassRowIndex, totalClassColIndex).Value = groupedClass.Sum(g => g.Count()); // Grand Total
            worksheet.Row(totalClassRowIndex).Style.Font.Bold = true;

            // HEADERS
            // COUNT OF GENDER
            int genderRowIndex = totalClassRowIndex + 2;
            worksheet.Cell(genderRowIndex, 1).Value = "COUNT OF GENDER";
            worksheet.Cell(genderRowIndex + 1, 1).Value = "Class";
            worksheet.Cell(genderRowIndex + 1, 2).Value = "F";
            worksheet.Cell(genderRowIndex + 1, 3).Value = "M";
            worksheet.Cell(genderRowIndex + 1, 4).Value = "Grand Total";

            // Prepare data grouped
            var groupedGender = generalAdmissions
                .GroupBy(d => d.Class)
                .OrderBy(g => g.Key)
                .ToList();

            int genderDataRowIndex = genderRowIndex + 2;
            foreach (var group in groupedGender)
            {
                worksheet.Cell(genderDataRowIndex, 1).Value = group.Key;

                int femaleCount = group.Count(d => d.Class == group.Key && d.Gender.ToUpper() == "FEMALE");
                int maleCount = group.Count(d => d.Class == group.Key && d.Gender.ToUpper() == "MALE");

                worksheet.Cell(genderDataRowIndex, 2).Value = femaleCount;
                worksheet.Cell(genderDataRowIndex, 3).Value = maleCount;
                worksheet.Cell(genderDataRowIndex, 4).Value = femaleCount + maleCount; // Grand Total

                genderDataRowIndex++;
            }

            int totalGenderRowIndex = genderDataRowIndex;
            worksheet.Cell(totalGenderRowIndex, 1).Value = "Total";
            worksheet.Cell(totalGenderRowIndex, 2).Value = generalAdmissions.Count(d => d.Gender.ToUpper() == "FEMALE");
            worksheet.Cell(totalGenderRowIndex, 3).Value = generalAdmissions.Count(d => d.Gender.ToUpper() == "MALE");
            worksheet.Cell(totalGenderRowIndex, 4).Value = generalAdmissions.Count(d => d.Gender.ToUpper() == "FEMALE") + generalAdmissions.Count(d => d.Gender.ToUpper() == "MALE");
            worksheet.Row(totalGenderRowIndex).Style.Font.Bold = true;

            // HEADERS
            // COUNT OF Eco. Stat
            int economicStatusRowIndex = totalGenderRowIndex + 2;
            worksheet.Cell(economicStatusRowIndex, 1).Value = "COUNT OF Eco. Stat";
            worksheet.Cell(economicStatusRowIndex + 1, 1).Value = "Eco. Stat";
            worksheet.Cell(economicStatusRowIndex + 1, 2).Value = "Grand Total";

            // Prepare data grouped by Economic Status
            var groupedEconomicStatus = generalAdmissions
                .GroupBy(d => d.EconomicStatus)
                .OrderBy(g => g.Key)
                .ToList();

            int economicStatusDataRowIndex = economicStatusRowIndex + 2;
            foreach (var group in groupedEconomicStatus)
            {
                worksheet.Cell(economicStatusDataRowIndex, 1).Value = group.Key;
                worksheet.Cell(economicStatusDataRowIndex, 2).Value = group.Count(); // Grand Total
                economicStatusDataRowIndex++;
            }

            int totalEconomicStatusRowIndex = economicStatusDataRowIndex;
            worksheet.Cell(totalEconomicStatusRowIndex, 1).Value = "Total";
            worksheet.Cell(totalEconomicStatusRowIndex, 2).Value = groupedEconomicStatus.Sum(g => g.Count()); // Grand Total
            worksheet.Row(totalEconomicStatusRowIndex).Style.Font.Bold = true;

            // HEADERS
            // COUNT OF Marital Status
            int maritalStatusRowIndex = totalEconomicStatusRowIndex + 2;
            worksheet.Cell(maritalStatusRowIndex, 1).Value = "COUNT OF Marital Status";
            worksheet.Cell(maritalStatusRowIndex + 1, 1).Value = "M. Stat";
            worksheet.Cell(maritalStatusRowIndex + 1, 2).Value = "Grand Total";

            // Prepare data grouped by Marital Status
            var groupedMaritalStatus = generalAdmissions
                .GroupBy(d => d.MaritalStatus)
                .OrderBy(g => g.Key)
                .ToList();

            int maritalStatusDataRowIndex = maritalStatusRowIndex + 2;
            foreach (var group in groupedMaritalStatus)
            {
                worksheet.Cell(maritalStatusDataRowIndex, 1).Value = group.Key;
                worksheet.Cell(maritalStatusDataRowIndex, 2).Value = group.Count(); // Grand Total
                maritalStatusDataRowIndex++;
            }

            int totalMaritalStatusRowIndex = maritalStatusDataRowIndex;
            worksheet.Cell(totalMaritalStatusRowIndex, 1).Value = "Total";
            worksheet.Cell(totalMaritalStatusRowIndex, 2).Value = groupedMaritalStatus.Sum(g => g.Count()); // Grand Total
            worksheet.Row(totalMaritalStatusRowIndex).Style.Font.Bold = true;

            // HEADERS
            // COUNT OF REFERRAL
            int referralRowIndex = totalMaritalStatusRowIndex + 2;
            worksheet.Cell(referralRowIndex, 1).Value = "COUNT OF REFERRAL";
            worksheet.Cell(referralRowIndex + 1, 1).Value = "Referral";
            worksheet.Cell(referralRowIndex + 1, 2).Value = "Grand Total";

            // Prepare data grouped by Referral
            var groupedReferral = generalAdmissions
                .GroupBy(d => d.Referral)
                .OrderBy(g => g.Key)
                .ToList();

            int referralDataRowIndex = referralRowIndex + 2;
            foreach (var group in groupedReferral)
            {
                worksheet.Cell(referralDataRowIndex, 1).Value = group.Key;
                worksheet.Cell(referralDataRowIndex, 2).Value = group.Count(); // Grand Total
                referralDataRowIndex++;
            }

            int totalReferralRowIndex = referralDataRowIndex;
            worksheet.Cell(totalReferralRowIndex, 1).Value = "Total";
            worksheet.Cell(totalReferralRowIndex, 2).Value = groupedReferral.Sum(g => g.Count()); // Grand Total
            worksheet.Row(totalReferralRowIndex).Style.Font.Bold = true;

            // HEADERS
            // COUNT OF ORIGIN
            int originRowIndex = totalReferralRowIndex + 2;
            worksheet.Cell(originRowIndex, 1).Value = "COUNT OF ORIGIN";
            worksheet.Cell(originRowIndex + 1, 1).Value = "Origin";
            worksheet.Cell(originRowIndex + 1, 2).Value = "Grand Total";

            // Prepare data grouped by Origin
            var groupedOrigin = generalAdmissions
                .GroupBy(d => d.Origin)
                .OrderBy(g => g.Key)
                .ToList();

            int originDataRowIndex = originRowIndex + 2;
            foreach (var group in groupedOrigin)
            {
                worksheet.Cell(originDataRowIndex, 1).Value = group.Key;
                worksheet.Cell(originDataRowIndex, 2).Value = group.Count(); // Grand Total
                originDataRowIndex++;
            }

            int totalOriginRowIndex = originDataRowIndex;
            worksheet.Cell(totalOriginRowIndex, 1).Value = "Total";
            worksheet.Cell(totalOriginRowIndex, 2).Value = groupedOrigin.Sum(g => g.Count()); // Grand Total
            worksheet.Row(totalOriginRowIndex).Style.Font.Bold = true;

            // HEADERS
            // COUNT OF AGE
            int ageRowIndex = totalOriginRowIndex + 2;
            worksheet.Cell(ageRowIndex, 1).Value = "COUNT OF AGE";
            worksheet.Cell(ageRowIndex + 1, 1).Value = "Age";
            worksheet.Cell(ageRowIndex + 1, 2).Value = "Grand Total";

            // Prepare data grouped by Age
            var groupedAge = generalAdmissions
                .GroupBy(d => d.Age)
                .OrderBy(g => g.Key)
                .ToList();

            int ageDataRowIndex = ageRowIndex + 2;
            foreach (var group in groupedAge)
            {
                worksheet.Cell(ageDataRowIndex, 1).Value = group.Key;
                worksheet.Cell(ageDataRowIndex, 2).Value = group.Count(); // Grand Total
                ageDataRowIndex++;
            }

            int totalAgeRowIndex = ageDataRowIndex;
            worksheet.Cell(totalAgeRowIndex, 1).Value = "Total";
            worksheet.Cell(totalAgeRowIndex, 2).Value = groupedAge.Sum(g => g.Count()); // Grand Total
            worksheet.Row(totalAgeRowIndex).Style.Font.Bold = true;

            // HEADERS
            // COUNT OF PATIENT EDU. ATTAINMENT
            int patientEduRowIndex = totalAgeRowIndex + 2;
            worksheet.Cell(patientEduRowIndex, 1).Value = "COUNT OF PATIENT EDU. ATTAINMENT";
            worksheet.Cell(patientEduRowIndex + 1, 1).Value = "Patient";
            worksheet.Cell(patientEduRowIndex + 1, 2).Value = "Grand Total";

            // Prepare data grouped by Patient Educational Attainment
            var groupedPatientEdu = generalAdmissions
                .GroupBy(d => d.EducationalAttainment)
                .OrderBy(g => g.Key)
                .ToList();

            int patientEduDataRowIndex = patientEduRowIndex + 2;
            foreach (var group in groupedPatientEdu)
            {
                worksheet.Cell(patientEduDataRowIndex, 1).Value = group.Key;
                worksheet.Cell(patientEduDataRowIndex, 2).Value = group.Count(); // Grand Total
                patientEduDataRowIndex++;
            }

            int totalPatientEduRowIndex = patientEduDataRowIndex;
            worksheet.Cell(totalPatientEduRowIndex, 1).Value = "Total";
            worksheet.Cell(totalPatientEduRowIndex, 2).Value = groupedPatientEdu.Sum(g => g.Count()); // Grand Total
            worksheet.Row(totalPatientEduRowIndex).Style.Font.Bold = true;

            // HEADERS
            // COUNT OF FATHER EDU. ATTAINMENT
            int fatherEduRowIndex = totalPatientEduRowIndex + 2;
            worksheet.Cell(fatherEduRowIndex, 1).Value = "COUNT OF FATHER EDU. ATTAINMENT";
            worksheet.Cell(fatherEduRowIndex + 1, 1).Value = "Father";
            worksheet.Cell(fatherEduRowIndex + 1, 2).Value = "Grand Total";

            // Prepare data grouped by Father's Educational Attainment
            var groupedFatherEdu = generalAdmissions
                .GroupBy(d => d.FatherEducationalAttainment)
                .OrderBy(g => g.Key)
                .ToList();

            int fatherEduDataRowIndex = fatherEduRowIndex + 2;
            foreach (var group in groupedFatherEdu)
            {
                worksheet.Cell(fatherEduDataRowIndex, 1).Value = group.Key;
                worksheet.Cell(fatherEduDataRowIndex, 2).Value = group.Count(); // Grand Total
                fatherEduDataRowIndex++;
            }

            int totalFatherEduRowIndex = fatherEduDataRowIndex;
            worksheet.Cell(totalFatherEduRowIndex, 1).Value = "Total";
            worksheet.Cell(totalFatherEduRowIndex, 2).Value = groupedFatherEdu.Sum(g => g.Count()); // Grand Total
            worksheet.Row(totalFatherEduRowIndex).Style.Font.Bold = true;

            // HEADERS
            // COUNT OF MOTHER EDU. ATTAINMENT
            int motherEduRowIndex = totalFatherEduRowIndex + 2;
            worksheet.Cell(motherEduRowIndex, 1).Value = "COUNT OF MOTHER EDU. ATTAINMENT";
            worksheet.Cell(motherEduRowIndex + 1, 1).Value = "Mother";
            worksheet.Cell(motherEduRowIndex + 1, 2).Value = "Grand Total";

            // Prepare data grouped by Mother's Educational Attainment
            var groupedMotherEdu = generalAdmissions
                .GroupBy(d => d.MotherEducationalAttainment)
                .OrderBy(g => g.Key)
                .ToList();

            int motherEduDataRowIndex = motherEduRowIndex + 2;
            foreach (var group in groupedMotherEdu)
            {
                worksheet.Cell(motherEduDataRowIndex, 1).Value = group.Key;
                worksheet.Cell(motherEduDataRowIndex, 2).Value = group.Count(); // Grand Total
                motherEduDataRowIndex++;
            }

            int totalMotherEduRowIndex = motherEduDataRowIndex;
            worksheet.Cell(totalMotherEduRowIndex, 1).Value = "Total";
            worksheet.Cell(totalMotherEduRowIndex, 2).Value = groupedMotherEdu.Sum(g => g.Count()); // Grand Total
            worksheet.Row(totalMotherEduRowIndex).Style.Font.Bold = true;

            // HEADERS
            // COUNT OF OLD/NEW
            int oldNewRowIndex = totalMotherEduRowIndex + 2;
            worksheet.Cell(oldNewRowIndex, 1).Value = "COUNT OF OLD/NEW";
            worksheet.Cell(oldNewRowIndex + 1, 1).Value = "Old/New";
            worksheet.Cell(oldNewRowIndex + 1, 2).Value = "Grand Total";

            int oldNewDataRowIndex = oldNewRowIndex + 2;
            worksheet.Cell(oldNewDataRowIndex, 1).Value = "New";
            worksheet.Cell(oldNewDataRowIndex, 2).Value = generalAdmissions.Count(g => !g.isOld);
            oldNewDataRowIndex++;
            worksheet.Cell(oldNewDataRowIndex, 1).Value = "Old";
            worksheet.Cell(oldNewDataRowIndex, 2).Value = generalAdmissions.Count(g => g.isOld);
            oldNewDataRowIndex++;

            worksheet.Cell(oldNewDataRowIndex, 1).Value = "Total";
            worksheet.Cell(oldNewDataRowIndex, 2).Value = generalAdmissions.Count(); // Grand Total
            worksheet.Row(oldNewDataRowIndex).Style.Font.Bold = true;

            // HEADERS
            // COUNT OF PWD
            int pwdRowIndex = oldNewDataRowIndex + 2;
            worksheet.Cell(pwdRowIndex, 1).Value = "COUNT OF PWD";
            worksheet.Cell(pwdRowIndex + 1, 1).Value = "PWD";
            worksheet.Cell(pwdRowIndex + 1, 2).Value = "Grand Total";

            int pwdDataRowIndex = pwdRowIndex + 2;
            worksheet.Cell(pwdDataRowIndex, 1).Value = "No";
            worksheet.Cell(pwdDataRowIndex, 2).Value = generalAdmissions.Count(g => !g.isPWD);
            pwdDataRowIndex++;
            worksheet.Cell(pwdDataRowIndex, 1).Value = "Yes";
            worksheet.Cell(pwdDataRowIndex, 2).Value = generalAdmissions.Count(g => g.isPWD);
            pwdDataRowIndex++;

            worksheet.Cell(pwdDataRowIndex, 1).Value = "Total";
            worksheet.Cell(pwdDataRowIndex, 2).Value = generalAdmissions.Count(); // Grand Total
            worksheet.Row(pwdDataRowIndex).Style.Font.Bold = true;

            // HEADERS
            // COUNT OF HOUSEHOLD SIZE
            int householdSizeRowIndex = pwdDataRowIndex + 2;
            worksheet.Cell(householdSizeRowIndex, 1).Value = "COUNT OF HOUSEHOLD SIZE";
            worksheet.Cell(householdSizeRowIndex + 1, 1).Value = "HH Size";
            worksheet.Cell(householdSizeRowIndex + 1, 2).Value = "Grand Total";

            // Prepare data grouped by Household Size
            var groupedHouseholdSize = generalAdmissions
                .GroupBy(d => d.HouseholdSize)
                .OrderBy(g => g.Key)
                .ToList();

            int householdSizeDataRowIndex = householdSizeRowIndex + 2;
            foreach (var group in groupedHouseholdSize)
            {
                worksheet.Cell(householdSizeDataRowIndex, 1).Value = group.Key;
                worksheet.Cell(householdSizeDataRowIndex, 2).Value = group.Count(); // Grand Total
                householdSizeDataRowIndex++;
            }

            int totalHouseholdSizeRowIndex = householdSizeDataRowIndex;
            worksheet.Cell(totalHouseholdSizeRowIndex, 1).Value = "Total";
            worksheet.Cell(totalHouseholdSizeRowIndex, 2).Value = groupedHouseholdSize.Sum(g => g.Count()); // Grand Total
            worksheet.Row(totalHouseholdSizeRowIndex).Style.Font.Bold = true;

            // HEADERS
            // COUNT OF LIGHT SOURCE
            int lightSourceRowIndex = totalHouseholdSizeRowIndex + 2;
            worksheet.Cell(lightSourceRowIndex, 1).Value = "COUNT OF LIGHT SOURCE";
            worksheet.Cell(lightSourceRowIndex + 1, 1).Value = "Light";
            worksheet.Cell(lightSourceRowIndex + 1, 2).Value = "Grand Total";

            // Prepare data grouped by Light Source
            var groupedLightSource = generalAdmissions
                .GroupBy(d => d.LightSource)
                .OrderBy(g => g.Key)
                .ToList();

            int lightSourceDataRowIndex = lightSourceRowIndex + 2;
            foreach (var group in groupedLightSource)
            {
                worksheet.Cell(lightSourceDataRowIndex, 1).Value = group.Key;
                worksheet.Cell(lightSourceDataRowIndex, 2).Value = group.Count(); // Grand Total
                lightSourceDataRowIndex++;
            }

            int totalLightSourceRowIndex = lightSourceDataRowIndex;
            worksheet.Cell(totalLightSourceRowIndex, 1).Value = "Total";
            worksheet.Cell(totalLightSourceRowIndex, 2).Value = groupedLightSource.Sum(g => g.Count()); // Grand Total
            worksheet.Row(totalLightSourceRowIndex).Style.Font.Bold = true;

            // HEADERS
            // COUNT OF WATER SOURCE
            int waterSourceRowIndex = totalLightSourceRowIndex + 2;
            worksheet.Cell(waterSourceRowIndex, 1).Value = "COUNT OF WATER SOURCE";
            worksheet.Cell(waterSourceRowIndex + 1, 1).Value = "Water";
            worksheet.Cell(waterSourceRowIndex + 1, 2).Value = "Grand Total";

            // Prepare data grouped by Water Source
            var groupedWaterSource = generalAdmissions
                .GroupBy(d => d.WaterSource)
                .OrderBy(g => g.Key)
                .ToList();

            int waterSourceDataRowIndex = waterSourceRowIndex + 2;
            foreach (var group in groupedWaterSource)
            {
                worksheet.Cell(waterSourceDataRowIndex, 1).Value = group.Key;
                worksheet.Cell(waterSourceDataRowIndex, 2).Value = group.Count(); // Grand Total
                waterSourceDataRowIndex++;
            }

            int totalWaterSourceRowIndex = waterSourceDataRowIndex;
            worksheet.Cell(totalWaterSourceRowIndex, 1).Value = "Total";
            worksheet.Cell(totalWaterSourceRowIndex, 2).Value = groupedWaterSource.Sum(g => g.Count()); // Grand Total
            worksheet.Row(totalWaterSourceRowIndex).Style.Font.Bold = true;

            // HEADERS
            // COUNT OF FUEL SOURCE
            int fuelSourceRowIndex = totalWaterSourceRowIndex + 2;
            worksheet.Cell(fuelSourceRowIndex, 1).Value = "COUNT OF FUEL SOURCE";
            worksheet.Cell(fuelSourceRowIndex + 1, 1).Value = "Fuel";
            worksheet.Cell(fuelSourceRowIndex + 1, 2).Value = "Grand Total";

            // Prepare data grouped by Fuel Source
            var groupedFuelSource = generalAdmissions
                .GroupBy(d => d.FuelSource)
                .OrderBy(g => g.Key)
                .ToList();

            int fuelSourceDataRowIndex = fuelSourceRowIndex + 2;
            foreach (var group in groupedFuelSource)
            {
                worksheet.Cell(fuelSourceDataRowIndex, 1).Value = group.Key;
                worksheet.Cell(fuelSourceDataRowIndex, 2).Value = group.Count(); // Grand Total
                fuelSourceDataRowIndex++;
            }

            int totalFuelSourceRowIndex = fuelSourceDataRowIndex;
            worksheet.Cell(totalFuelSourceRowIndex, 1).Value = "Total";
            worksheet.Cell(totalFuelSourceRowIndex, 2).Value = groupedFuelSource.Sum(g => g.Count()); // Grand Total
            worksheet.Row(totalFuelSourceRowIndex).Style.Font.Bold = true;

            // HEADERS
            // COUNT OF DWELLING TYPE
            int dwellingTypeRowIndex = totalFuelSourceRowIndex + 2;
            worksheet.Cell(dwellingTypeRowIndex, 1).Value = "COUNT OF DWELLING TYPE";
            worksheet.Cell(dwellingTypeRowIndex + 1, 1).Value = "Dwell";
            worksheet.Cell(dwellingTypeRowIndex + 1, 2).Value = "Grand Total";

            // Prepare data grouped by Dwelling Type
            var groupedDwellingType = generalAdmissions
                .GroupBy(d => d.DwellingType)
                .OrderBy(g => g.Key)
                .ToList();

            int dwellingTypeDataRowIndex = dwellingTypeRowIndex + 2;
            foreach (var group in groupedDwellingType)
            {
                worksheet.Cell(dwellingTypeDataRowIndex, 1).Value = group.Key;
                worksheet.Cell(dwellingTypeDataRowIndex, 2).Value = group.Count(); // Grand Total
                dwellingTypeDataRowIndex++;
            }

            int totalDwellingTypeRowIndex = dwellingTypeDataRowIndex;
            worksheet.Cell(totalDwellingTypeRowIndex, 1).Value = "Total";
            worksheet.Cell(totalDwellingTypeRowIndex, 2).Value = groupedDwellingType.Sum(g => g.Count()); // Grand Total
            worksheet.Row(totalDwellingTypeRowIndex).Style.Font.Bold = true;

            // HEADERS
            // COUNT OF STATS OCCUPATION
            int statsOccupationRowIndex = totalDwellingTypeRowIndex + 2;
            worksheet.Cell(statsOccupationRowIndex, 1).Value = "COUNT OF STATS OCCUPATION";
            worksheet.Cell(statsOccupationRowIndex + 1, 1).Value = "Stat's Occupation";
            worksheet.Cell(statsOccupationRowIndex + 1, 2).Value = "Grand Total";

            // Prepare data grouped by Stats Occupation
            var groupedStatsOccupation = generalAdmissions
                .GroupBy(d => d.StatsOccupation)
                .OrderBy(g => g.Key)
                .ToList();

            int statsOccupationDataRowIndex = statsOccupationRowIndex + 2;
            foreach (var group in groupedStatsOccupation)
            {
                worksheet.Cell(statsOccupationDataRowIndex, 1).Value = group.Key;
                worksheet.Cell(statsOccupationDataRowIndex, 2).Value = group.Count(); // Grand Total
                statsOccupationDataRowIndex++;
            }

            int totalStatsOccupationRowIndex = statsOccupationDataRowIndex;
            worksheet.Cell(totalStatsOccupationRowIndex, 1).Value = "Total";
            worksheet.Cell(totalStatsOccupationRowIndex, 2).Value = groupedStatsOccupation.Sum(g => g.Count()); // Grand Total
            worksheet.Row(totalStatsOccupationRowIndex).Style.Font.Bold = true;

            // HEADERS
            // COUNT OF INCOME RANGE
            int incomeRangeRowIndex = totalStatsOccupationRowIndex + 2;
            worksheet.Cell(incomeRangeRowIndex, 1).Value = "COUNT OF INCOME RANGE";
            worksheet.Cell(incomeRangeRowIndex + 1, 1).Value = "Income Range";
            worksheet.Cell(incomeRangeRowIndex + 1, 2).Value = "Grand Total";

            // Prepare data grouped by Income Range
            var groupedIncomeRange = generalAdmissions
                .GroupBy(d => d.IncomeRange)
                .OrderBy(g => g.Key)
                .ToList();

            int incomeRangeDataRowIndex = incomeRangeRowIndex + 2;
            foreach (var group in groupedIncomeRange)
            {
                worksheet.Cell(incomeRangeDataRowIndex, 1).Value = group.Key;
                worksheet.Cell(incomeRangeDataRowIndex, 2).Value = group.Count(); // Grand Total
                incomeRangeDataRowIndex++;
            }

            int totalIncomeRangeRowIndex = incomeRangeDataRowIndex;
            worksheet.Cell(totalIncomeRangeRowIndex, 1).Value = "Total";
            worksheet.Cell(totalIncomeRangeRowIndex, 2).Value = groupedIncomeRange.Sum(g => g.Count()); // Grand Total
            worksheet.Row(totalIncomeRangeRowIndex).Style.Font.Bold = true;

            // HEADERS
            // COUNT OF OCCUPATION
            int occupationRowIndex = totalIncomeRangeRowIndex + 2;
            worksheet.Cell(occupationRowIndex, 1).Value = "COUNT OF OCCUPATION";
            worksheet.Cell(occupationRowIndex + 1, 1).Value = "Occupation";
            worksheet.Cell(occupationRowIndex + 1, 2).Value = "Grand Total";

            // Prepare data grouped by Occupation
            var groupedOccupation = generalAdmissions
                .GroupBy(d => d.Occupation)
                .OrderBy(g => g.Key)
                .ToList();

            int occupationDataRowIndex = occupationRowIndex + 2;
            foreach (var group in groupedOccupation)
            {
                worksheet.Cell(occupationDataRowIndex, 1).Value = group.Key;
                worksheet.Cell(occupationDataRowIndex, 2).Value = group.Count(); // Grand Total
                occupationDataRowIndex++;
            }

            int totalOccupationRowIndex = occupationDataRowIndex;
            worksheet.Cell(totalOccupationRowIndex, 1).Value = "Total";
            worksheet.Cell(totalOccupationRowIndex, 2).Value = groupedOccupation.Sum(g => g.Count()); // Grand Total
            worksheet.Row(totalOccupationRowIndex).Style.Font.Bold = true;

            // HEADERS
            // COUNT OF MONTHLY INCOME
            int monthlyIncomeRowIndex = totalOccupationRowIndex + 2;
            worksheet.Cell(monthlyIncomeRowIndex, 1).Value = "COUNT OF MONTHLY INCOME";
            worksheet.Cell(monthlyIncomeRowIndex + 1, 1).Value = "Monthly Income";
            worksheet.Cell(monthlyIncomeRowIndex + 1, 2).Value = "Grand Total";

            // Prepare data grouped by Monthly Income
            var groupedMonthlyIncome = generalAdmissions
                .GroupBy(d => d.MonthlyIncome)
                .OrderBy(g => g.Key)
                .ToList();

            int monthlyIncomeDataRowIndex = monthlyIncomeRowIndex + 2;
            foreach (var group in groupedMonthlyIncome)
            {
                worksheet.Cell(monthlyIncomeDataRowIndex, 1).Value = group.Key;
                worksheet.Cell(monthlyIncomeDataRowIndex, 2).Value = group.Count(); // Grand Total
                monthlyIncomeDataRowIndex++;
            }

            int totalMonthlyIncomeRowIndex = monthlyIncomeDataRowIndex;
            worksheet.Cell(totalMonthlyIncomeRowIndex, 1).Value = "Total";
            worksheet.Cell(totalMonthlyIncomeRowIndex, 2).Value = groupedMonthlyIncome.Sum(g => g.Count()); // Grand Total
            worksheet.Row(totalMonthlyIncomeRowIndex).Style.Font.Bold = true;

            // Column 1
            var cell2 = worksheet.Cell(1, 1);
            cell2.Value = "GA Reports";
            cell2.Style.Font.Bold = true;
            cell2.Style.Font.FontSize = 12;
            cell2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell2.Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Range(1, 1, 1, worksheet.LastColumnUsed().ColumnNumber()).Merge();

            // Column 2
            var cell3 = worksheet.Cell(2, 1);
            cell3.Value = $"{monthLabel} GA";
            cell3.Style.Font.Bold = true;
            cell3.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell3.Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Range(2, 1, 2, worksheet.LastColumnUsed().ColumnNumber()).Merge();

            // Set header row style 
            var rowsList = new List<int>
            {
                4, classRowIndex, genderRowIndex, economicStatusRowIndex, maritalStatusRowIndex, 
                referralRowIndex, originRowIndex, ageRowIndex, patientEduRowIndex, fatherEduRowIndex, 
                motherEduRowIndex, oldNewRowIndex, pwdRowIndex, householdSizeRowIndex, lightSourceRowIndex, 
                waterSourceRowIndex, fuelSourceRowIndex, dwellingTypeRowIndex, statsOccupationRowIndex, 
                incomeRangeRowIndex, occupationRowIndex, monthlyIncomeRowIndex
            };

            var headerRowsList = new List<int>
            {
                5, classRowIndex + 1, genderRowIndex + 1, economicStatusRowIndex + 1, maritalStatusRowIndex + 1, 
                referralRowIndex + 1, originRowIndex + 1, ageRowIndex + 1, patientEduRowIndex + 1, 
                fatherEduRowIndex + 1, motherEduRowIndex + 1, oldNewRowIndex + 1, pwdRowIndex + 1, 
                householdSizeRowIndex + 1, lightSourceRowIndex + 1, waterSourceRowIndex + 1, 
                fuelSourceRowIndex + 1, dwellingTypeRowIndex + 1, statsOccupationRowIndex + 1, 
                incomeRangeRowIndex + 1, occupationRowIndex + 1, monthlyIncomeRowIndex + 1

            };

            var totalRowsList = new List<int>
            {
                totalDateRowIndex, totalClassRowIndex, totalGenderRowIndex, totalEconomicStatusRowIndex, 
                totalMaritalStatusRowIndex, totalReferralRowIndex, totalOriginRowIndex, totalAgeRowIndex, 
                totalPatientEduRowIndex, totalFatherEduRowIndex, totalMotherEduRowIndex, oldNewRowIndex + 4, 
                pwdRowIndex + 4, totalHouseholdSizeRowIndex, totalLightSourceRowIndex, totalWaterSourceRowIndex, 
                totalFuelSourceRowIndex, totalDwellingTypeRowIndex, totalStatsOccupationRowIndex, 
                totalIncomeRangeRowIndex, totalOccupationRowIndex, totalMonthlyIncomeRowIndex
            };

            var userNameClaim = User.FindFirst(ClaimTypes.Name).Value;

            ExcelReportStyler.ApplyWorksheetDesign(worksheet, rowsList, headerRowsList, totalRowsList, monthlyIncomeDataRowIndex, userNameClaim, true);

            // Autofit for better presentation
            worksheet.Columns().AdjustToContents();

            await using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                stream.Position = 0;
                return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"{fileName}.xlsx");
            }

        }

        public async Task<IActionResult> ExportPatientInterviewedToExcel(int userID, string? month)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            // Parse the month input if provided
            bool filterByMonth = DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedMonth);

            var query = context.GeneralAdmission.AsQueryable();

            if (userID > 0)
            {
                query = query.Where(ga => ga.UserID == userID);
            }

            if (filterByMonth)
            {
                query = query.Where(ga => ga.Date.Month == parsedMonth.Month && ga.Date.Year == parsedMonth.Year);
            }

            var generalAdmissions = await query.ToListAsync();

            if (generalAdmissions == null || !generalAdmissions.Any())
            {
                TempData["ErrorMessage"] = "No General Admission records found for selected filters.";
                return RedirectToAction("Reports");
            }

            // File name generation
            string mswName = userID > 0 ? generalAdmissions.First().MSW : "All MSW";
            string monthLabel = filterByMonth ? parsedMonth.ToString("MMMM_yyyy") : generalAdmissions.First().Date.Year.ToString();
            string fileName = $"GA_PatientInterviewed_{monthLabel}_{mswName}";

            // Sanitize sheet name (for Excel)
            string safeSheetName = Regex.Replace(fileName, @"[\[\]\*\?/\\:]", "_");
            if (safeSheetName.Length > 31)
                safeSheetName = safeSheetName.Substring(0, 31);

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(safeSheetName);

            // HEADERS

            var headers = new[]
            {
                "MSW", "Date", "Name of Patient", "Ward"
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
            cell2.Value = "Patient Interviewed Per MSW";
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

            int dataRow = headerRow + 1;
            foreach (var ga in generalAdmissions)
            {
                worksheet.Cell(dataRow, 1).Value = ga.MSW;
                worksheet.Cell(dataRow, 2).Value = ga.Date.ToShortDateString();
                worksheet.Cell(dataRow, 3).Value = $"{ga.LastName}, {ga.FirstName} {ga.MiddleName}";
                worksheet.Cell(dataRow, 4).Value = ga.Ward;

                dataRow++;
            }

            ExcelReportStyler.ApplyWorksheetDesign(worksheet, new List<int> { 1, 2, 3 }, new List<int> { headerRow }, new List<int> { dataRow }, dataRow, User.FindFirst(ClaimTypes.Name).Value, false, true);

            // Autofit for better presentation
            worksheet.Columns().AdjustToContents();

            await using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                stream.Position = 0;
                return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"{fileName}.xlsx");
            }
        }

        public async Task<IActionResult> ExportTotalInterviewedToExcel(int userID, string? month)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            // Parse the month input if provided
            bool filterByMonth = DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedMonth);

            var query = context.GeneralAdmission.AsQueryable();

            if (userID > 0)
            {
                query = query.Where(g => g.UserID == userID);
            }

            if (filterByMonth)
            {
                query = query.Where(g => g.Date.Month == parsedMonth.Month && g.Date.Year == parsedMonth.Year);
            }

            var generalAdmissions = await query.ToListAsync();

            if (generalAdmissions == null || !generalAdmissions.Any())
            {
                TempData["ErrorMessage"] = "No General Admissions records found for selected filters.";
                return RedirectToAction("Reports");
            }

            // File name generation
            string mswName = userID > 0 ? generalAdmissions.First().MSW : "All";
            string monthLabel = filterByMonth ? parsedMonth.ToString("MMMM_yyyy") : generalAdmissions.First().Date.Year.ToString();
            string fileName = $"GA_TotalInterviewed_{monthLabel}";

            // Sanitize file name (for download)
            string safeFileName = Regex.Replace(fileName, @"[^\w\-]", "_");

            // Sanitize sheet name (for Excel)
            string safeSheetName = Regex.Replace(fileName, @"[\[\]\*\?/\\:]", "_");
            if (safeSheetName.Length > 31)
                safeSheetName = safeSheetName.Substring(0, 31);

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(safeSheetName);

            //var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            //var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();
            var users = await context.Users.ToListAsync();

            // HEADERS
            // COUNT OF DATE PROCESSED BY MSW
            worksheet.Cell(4, 1).Value = "COUNT OF DATE PROCESSED BY MSW";
            worksheet.Cell(5, 1).Value = "Date Processed by MSW";

            int dateColIndex = 2;
            foreach (var user in users)
            {
                worksheet.Cell(5, dateColIndex).Value = user.Username;
                dateColIndex++;
            }

            worksheet.Cell(5, dateColIndex).Value = "Grand Total";

            // Prepare data grouped by ProcessedDate
            var groupedOPD = generalAdmissions
                .GroupBy(d => d.Date)
                .OrderBy(g => g.Key)
                .ToList();

            int dateRowIndex = 6;
            foreach (var group in groupedOPD)
            {
                worksheet.Cell(dateRowIndex, 1).Value = group.Key.ToShortDateString();

                int colIndex = 2;
                foreach (var user in users)
                {
                    var count = group.Count(d => d.UserID == user.UserID);
                    worksheet.Cell(dateRowIndex, colIndex).Value = count;
                    colIndex++;
                }

                worksheet.Cell(dateRowIndex, colIndex).Value = group.Count(); // Grand Total
                dateRowIndex++;
            }

            int totalDateRowIndex = dateRowIndex;
            worksheet.Cell(totalDateRowIndex, 1).Value = "Total";

            int totalDateColIndex = 2;
            foreach (var user in users)
            {
                var totalCount = groupedOPD.Sum(g => g.Count(d => d.UserID == user.UserID));
                worksheet.Cell(totalDateRowIndex, totalDateColIndex).Value = totalCount;
                totalDateColIndex++;
            }

            worksheet.Cell(totalDateRowIndex, totalDateColIndex).Value = groupedOPD.Sum(g => g.Count()); // Grand Total
            worksheet.Row(totalDateRowIndex).Style.Font.Bold = true;

            // Column 1
            var cell2 = worksheet.Cell(1, 1);
            cell2.Value = "GA Total Interviewed";
            cell2.Style.Font.Bold = true;
            cell2.Style.Font.FontSize = 12;
            cell2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell2.Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Range(1, 1, 1, worksheet.LastColumnUsed().ColumnNumber()).Merge();

            // Column 2
            var cell3 = worksheet.Cell(2, 1);
            cell3.Value = $"{monthLabel} GA";
            cell3.Style.Font.Bold = true;
            cell3.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell3.Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Range(2, 1, 2, worksheet.LastColumnUsed().ColumnNumber()).Merge();

            // Set header row style 
            var rowsList = new List<int>
            {
                4
            };

            var headerRowsList = new List<int>
            {
                5
            };

            var totalRowsList = new List<int>
            {
                totalDateRowIndex
            };

            var userNameClaim = User.FindFirst(ClaimTypes.Name).Value;

            ExcelReportStyler.ApplyWorksheetDesign(worksheet, rowsList, headerRowsList, totalRowsList, totalDateRowIndex, userNameClaim, true);

            // Autofit for better presentation
            worksheet.Columns().AdjustToContents();

            await using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                stream.Position = 0;
                return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"{fileName}.xlsx");
            }
        }

        public async Task<IActionResult> ExportReferralOldNewPWDToExcel(int userID, string? month)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            // Parse the month input if provided
            bool filterByMonth = DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedMonth);

            var query = context.GeneralAdmission.AsQueryable();

            if (userID > 0)
            {
                query = query.Where(g => g.UserID == userID);
            }

            if (filterByMonth)
            {
                query = query.Where(g => g.Date.Month == parsedMonth.Month && g.Date.Year == parsedMonth.Year);
            }

            var generalAdmissions = await query.ToListAsync();

            if (generalAdmissions == null || !generalAdmissions.Any())
            {
                TempData["ErrorMessage"] = "No General Admissions records found for selected filters.";
                return RedirectToAction("Reports");
            }

            // File name generation
            string mswName = userID > 0 ? generalAdmissions.First().MSW : "All";
            string monthLabel = filterByMonth ? parsedMonth.ToString("MMMM_yyyy") : generalAdmissions.First().Date.Year.ToString();
            string fileName = $"GA_ReferralOldNewPWD_{monthLabel}";

            // Sanitize file name (for download)
            string safeFileName = Regex.Replace(fileName, @"[^\w\-]", "_");

            // Sanitize sheet name (for Excel)
            string safeSheetName = Regex.Replace(fileName, @"[\[\]\*\?/\\:]", "_");
            if (safeSheetName.Length > 31)
                safeSheetName = safeSheetName.Substring(0, 31);

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(safeSheetName);

            //var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            //var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();
            var users = await context.Users.ToListAsync();

            // HEADERS
            // COUNT OF DATE PROCESSED BY MSW
            worksheet.Cell(4, 1).Value = "COUNT OF REFERRAL BY MSW";
            worksheet.Cell(5, 1).Value = "Referral";

            int referralColIndex = 2;
            foreach (var user in users)
            {
                worksheet.Cell(5, referralColIndex).Value = user.Username;
                referralColIndex++;
            }

            worksheet.Cell(5, referralColIndex).Value = "Grand Total";

            // Prepare data grouped by ProcessedDate
            var groupedOPD = generalAdmissions
                .GroupBy(d => d.Referral)
                .OrderBy(g => g.Key)
                .ToList();

            int dateRowIndex = 6;
            foreach (var group in groupedOPD)
            {
                worksheet.Cell(dateRowIndex, 1).Value = group.Key;

                int colIndex = 2;
                foreach (var user in users)
                {
                    var count = group.Count(d => d.UserID == user.UserID);
                    worksheet.Cell(dateRowIndex, colIndex).Value = count;
                    colIndex++;
                }

                worksheet.Cell(dateRowIndex, colIndex).Value = group.Count(); // Grand Total
                dateRowIndex++;
            }

            int totalDateRowIndex = dateRowIndex;
            worksheet.Cell(totalDateRowIndex, 1).Value = "Total";

            int totalDateColIndex = 2;
            foreach (var user in users)
            {
                var totalCount = groupedOPD.Sum(g => g.Count(d => d.UserID == user.UserID));
                worksheet.Cell(totalDateRowIndex, totalDateColIndex).Value = totalCount;
                totalDateColIndex++;
            }

            worksheet.Cell(totalDateRowIndex, totalDateColIndex).Value = groupedOPD.Sum(g => g.Count()); // Grand Total
            worksheet.Row(totalDateRowIndex).Style.Font.Bold = true;

            // COUNT OF OLD/NEW
            int oldNewStartRowIndex = totalDateRowIndex + 2;
            worksheet.Cell(oldNewStartRowIndex, 1).Value = "COUNT OF OLD/NEW";
            worksheet.Cell(oldNewStartRowIndex + 1, 1).Value = "Old/New";

            int oldNewDataColIndex = 2;
            foreach (var user in users)
            {
                worksheet.Cell(oldNewStartRowIndex + 1, oldNewDataColIndex).Value = user.Username;
                oldNewDataColIndex++;
            }

            worksheet.Cell(oldNewStartRowIndex + 1, oldNewDataColIndex).Value = "Grand Total";

            int oldNewDataRowIndex = oldNewStartRowIndex + 2;
            worksheet.Cell(oldNewDataRowIndex, 1).Value = "New";
            int colIndexNew = 2;
            foreach (var user in users)
            {
                var countNew = generalAdmissions.Count(g => !g.isOld && g.UserID == user.UserID);
                worksheet.Cell(oldNewDataRowIndex, colIndexNew).Value = countNew;
                colIndexNew++;
            }
            worksheet.Cell(oldNewDataRowIndex, colIndexNew).Value = generalAdmissions.Count(g => !g.isOld);

            oldNewDataRowIndex++;

            worksheet.Cell(oldNewDataRowIndex, 1).Value = "Old";
            int colIndexOld = 2;
            foreach (var user in users)
            {
                var countOld = generalAdmissions.Count(g => g.isOld && g.UserID == user.UserID);
                worksheet.Cell(oldNewDataRowIndex, colIndexOld).Value = countOld;
                colIndexOld++;
            }
            worksheet.Cell(oldNewDataRowIndex, colIndexOld).Value = generalAdmissions.Count(g => g.isOld);

            oldNewDataRowIndex++;

            worksheet.Cell(oldNewDataRowIndex, 1).Value = "Total";
            int colIndexTotal = 2;
            foreach (var user in users)
            {
                var countTotal = generalAdmissions.Count(g => g.UserID == user.UserID);
                worksheet.Cell(oldNewDataRowIndex, colIndexTotal).Value = countTotal;
                colIndexTotal++;
            }
            worksheet.Cell(oldNewDataRowIndex, colIndexTotal).Value = generalAdmissions.Count();
            worksheet.Row(oldNewDataRowIndex).Style.Font.Bold = true;

            // COUNT OF PWD
            int pwdStartRowIndex = oldNewDataRowIndex + 2;
            worksheet.Cell(pwdStartRowIndex, 1).Value = "COUNT OF PWD";
            worksheet.Cell(pwdStartRowIndex + 1, 1).Value = "PWD";

            int pwdDataColIndex = 2;
            foreach (var user in users)
            {
                worksheet.Cell(pwdStartRowIndex + 1, pwdDataColIndex).Value = user.Username;
                pwdDataColIndex++;
            }

            worksheet.Cell(pwdStartRowIndex + 1, pwdDataColIndex).Value = "Grand Total";

            int pwdDataRowIndex = pwdStartRowIndex + 2;
            worksheet.Cell(pwdDataRowIndex, 1).Value = "N";
            int colIndexNo = 2;
            foreach (var user in users)
            {
                var countNo = generalAdmissions.Count(g => !g.isPWD && g.UserID == user.UserID);
                worksheet.Cell(pwdDataRowIndex, colIndexNo).Value = countNo;
                colIndexNo++;
            }
            worksheet.Cell(pwdDataRowIndex, colIndexNo).Value = generalAdmissions.Count(g => !g.isPWD);

            pwdDataRowIndex++;

            worksheet.Cell(pwdDataRowIndex, 1).Value = "Y";
            int colIndexYes = 2;
            foreach (var user in users)
            {
                var countYes = generalAdmissions.Count(g => g.isPWD && g.UserID == user.UserID);
                worksheet.Cell(pwdDataRowIndex, colIndexYes).Value = countYes;
                colIndexYes++;
            }
            worksheet.Cell(pwdDataRowIndex, colIndexYes).Value = generalAdmissions.Count(g => g.isPWD);

            pwdDataRowIndex++;

            worksheet.Cell(pwdDataRowIndex, 1).Value = "Total";
            int colIndexTotalPWD = 2;
            foreach (var user in users)
            {
                var countTotal = generalAdmissions.Count(g => g.UserID == user.UserID);
                worksheet.Cell(pwdDataRowIndex, colIndexTotalPWD).Value = countTotal;
                colIndexTotalPWD++;
            }
            worksheet.Cell(pwdDataRowIndex, colIndexTotalPWD).Value = generalAdmissions.Count();
            worksheet.Row(pwdDataRowIndex).Style.Font.Bold = true;

            // Column 1
            var cell2 = worksheet.Cell(1, 1);
            cell2.Value = "GA ReferralOldNewPWD";
            cell2.Style.Font.Bold = true;
            cell2.Style.Font.FontSize = 12;
            cell2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell2.Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Range(1, 1, 1, worksheet.LastColumnUsed().ColumnNumber()).Merge();

            // Column 2
            var cell3 = worksheet.Cell(2, 1);
            cell3.Value = $"{monthLabel} GA";
            cell3.Style.Font.Bold = true;
            cell3.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell3.Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Range(2, 1, 2, worksheet.LastColumnUsed().ColumnNumber()).Merge();

            // Set header row style 
            var rowsList = new List<int>
            {
                4, oldNewStartRowIndex, pwdStartRowIndex
            };

            var headerRowsList = new List<int>
            {
                5, oldNewStartRowIndex + 1, pwdStartRowIndex + 1
            };

            var totalRowsList = new List<int>
            {
                totalDateRowIndex, oldNewDataRowIndex, pwdDataRowIndex
            };

            var userNameClaim = User.FindFirst(ClaimTypes.Name).Value;

            ExcelReportStyler.ApplyWorksheetDesign(worksheet, rowsList, headerRowsList, totalRowsList, pwdDataRowIndex, userNameClaim, true);

            // Autofit for better presentation
            worksheet.Columns().AdjustToContents();

            await using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                stream.Position = 0;
                return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"{fileName}.xlsx");
            }
        }

        public async Task<IActionResult> ExportPHICToExcel(int userID, string? month)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            // Parse the month input if provided
            bool filterByMonth = DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedMonth);

            var query = context.GeneralAdmission.AsQueryable();

            if (userID > 0)
            {
                query = query.Where(g => g.UserID == userID);
            }

            if (filterByMonth)
            {
                query = query.Where(g => g.Date.Month == parsedMonth.Month && g.Date.Year == parsedMonth.Year);
            }

            var generalAdmissions = await query.ToListAsync();

            if (generalAdmissions == null || !generalAdmissions.Any())
            {
                TempData["ErrorMessage"] = "No General Admissions records found for selected filters.";
                return RedirectToAction("Reports");
            }

            // File name generation
            string mswName = userID > 0 ? generalAdmissions.First().MSW : "All";
            string monthLabel = filterByMonth ? parsedMonth.ToString("MMMM_yyyy") : generalAdmissions.First().Date.Year.ToString();
            string fileName = $"GA_PHIC_{monthLabel}";

            // Sanitize file name (for download)
            string safeFileName = Regex.Replace(fileName, @"[^\w\-]", "_");

            // Sanitize sheet name (for Excel)
            string safeSheetName = Regex.Replace(fileName, @"[\[\]\*\?/\\:]", "_");
            if (safeSheetName.Length > 31)
                safeSheetName = safeSheetName.Substring(0, 31);

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(safeSheetName);

            //var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            //var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();
            var users = await context.Users.ToListAsync();

            // HEADERS
            // COUNT OF DATE PROCESSED BY MSW
            worksheet.Cell(4, 1).Value = "COUNT OF PHIC BY MSW";
            worksheet.Cell(5, 1).Value = "PHIC";

            int referralColIndex = 2;
            foreach (var user in users)
            {
                worksheet.Cell(5, referralColIndex).Value = user.Username;
                referralColIndex++;
            }

            worksheet.Cell(5, referralColIndex).Value = "Grand Total";

            // Prepare data grouped by ProcessedDate
            var groupedOPD = generalAdmissions
                .GroupBy(d => d.PHIC)
                .OrderBy(g => g.Key)
                .ToList();

            int dateRowIndex = 6;
            foreach (var group in groupedOPD)
            {
                worksheet.Cell(dateRowIndex, 1).Value = group.Key;

                int colIndex = 2;
                foreach (var user in users)
                {
                    var count = group.Count(d => d.UserID == user.UserID);
                    worksheet.Cell(dateRowIndex, colIndex).Value = count;
                    colIndex++;
                }

                worksheet.Cell(dateRowIndex, colIndex).Value = group.Count(); // Grand Total
                dateRowIndex++;
            }

            int totalDateRowIndex = dateRowIndex;
            worksheet.Cell(totalDateRowIndex, 1).Value = "Total";

            int totalDateColIndex = 2;
            foreach (var user in users)
            {
                var totalCount = groupedOPD.Sum(g => g.Count(d => d.UserID == user.UserID));
                worksheet.Cell(totalDateRowIndex, totalDateColIndex).Value = totalCount;
                totalDateColIndex++;
            }

            worksheet.Cell(totalDateRowIndex, totalDateColIndex).Value = groupedOPD.Sum(g => g.Count()); // Grand Total
            worksheet.Row(totalDateRowIndex).Style.Font.Bold = true;

            // Column 1
            var cell2 = worksheet.Cell(1, 1);
            cell2.Value = "GA PHIC";
            cell2.Style.Font.Bold = true;
            cell2.Style.Font.FontSize = 12;
            cell2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell2.Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Range(1, 1, 1, worksheet.LastColumnUsed().ColumnNumber()).Merge();

            // Column 2
            var cell3 = worksheet.Cell(2, 1);
            cell3.Value = $"{monthLabel} GA";
            cell3.Style.Font.Bold = true;
            cell3.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell3.Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Range(2, 1, 2, worksheet.LastColumnUsed().ColumnNumber()).Merge();

            // Set header row style 
            var rowsList = new List<int>
            {
                4
            };

            var headerRowsList = new List<int>
            {
                5
            };

            var totalRowsList = new List<int>
            {
                totalDateRowIndex
            };

            var userNameClaim = User.FindFirst(ClaimTypes.Name).Value;

            ExcelReportStyler.ApplyWorksheetDesign(worksheet, rowsList, headerRowsList, totalRowsList, totalDateRowIndex, userNameClaim, true);

            // Autofit for better presentation
            worksheet.Columns().AdjustToContents();

            await using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                stream.Position = 0;
                return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"{fileName}.xlsx");
            }
        }

        public async Task<IActionResult> Statistics()
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var generalAdmissions = await context.GeneralAdmission.ToListAsync();
            if (generalAdmissions == null || !generalAdmissions.Any())
            {
                TempData["ErrorMessage"] = "No General Admission records found.";
                return RedirectToAction("Index");
            }

            var progressNotes = await context.ProgressNotes.ToListAsync();

            //var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            //var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();
            var users = await context.Users.ToListAsync();

            var viewModel = new GeneralAdmissionViewModel
            {
                Users = users,
                GeneralAdmissions = generalAdmissions,
                ProgressNotes = progressNotes
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ExportStatisticsToExcel(int userID, string? month)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            // Parse the month input if provided
            bool filterByMonth = DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedMonth);

            var query = context.GeneralAdmission.AsQueryable();

            if (userID > 0)
            {
                query = query.Where(ga => ga.UserID == userID);
            }

            if (filterByMonth)
            {
                query = query.Where(ga => ga.Date.Month == parsedMonth.Month && ga.Date.Year == parsedMonth.Year);
            }

            var generalAdmissions = await query.ToListAsync();

            if (generalAdmissions == null || !generalAdmissions.Any())
            {
                TempData["ErrorMessage"] = "No Admission records found for selected filters.";
                return RedirectToAction("Statistics");
            }

            // File name generation
            string mswName = userID > 0 ? generalAdmissions.First().MSW : "All MSW";
            string monthLabel = filterByMonth ? parsedMonth.ToString("MMMM_yyyy") : generalAdmissions.First().Date.Year.ToString();
            string fileName = $"GA_Statistics_{monthLabel}_{mswName}";

            // Sanitize sheet name (for Excel)
            string safeSheetName = Regex.Replace(fileName, @"[\[\]\*\?/\\:]", "_");
            if (safeSheetName.Length > 31)
                safeSheetName = safeSheetName.Substring(0, 31);

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(safeSheetName);

            // HEADERS

            var headers = new[]
            {
                "MONTH", "JAN", "FEB", "MARCH", "APRIL",
                "MAY", "JUNE", "TOTAL", "JULY", "AUG", "SEPT",
                "OCT", "NOV", "DEC", "TOTAL"
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
            cell2.Value = "General Admission Statistics";
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

            // SOURCE OF REFERRAL
            int referralRow = headerRow + 1;

            worksheet.Cell(referralRow, 1).Value = "I. SOURCE OF REFERRAL";
            worksheet.Cell(referralRow, 1).Style.Font.Bold = true;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(referralRow, i + 1).Value = 0;
                    worksheet.Cell(referralRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(referralRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(referralRow, i + 1).Value = "";
                }
            }

            referralRow++;

            worksheet.Cell(referralRow, 1).Value = "Referring Party";
            worksheet.Cell(referralRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(referralRow, 1).Style.Alignment.Indent = 1;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(referralRow, i + 1).Value = 0;
                    worksheet.Cell(referralRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(referralRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(referralRow, i + 1).Value = "";
                }
            }

            referralRow++;

            var sourceOfReferral = new Dictionary<string, string>
                    {
                                { "1. Government Hospital", "Govt. Hosp." },
                                { "2. Private Hospital", "Private/Clinic" },
                                { "3. Politicians", "Politicians" },
                                { "4. Media", "Media" },
                                { "5. Health Care Team", "Health Care Team" },
                                { "6. NGOs/Private Welfare Agencies", "NGO/Private Welfare" },
                                { "7. Government Agencies (DSWD, DOH Officials)", "Govt. Agencies" },
                                { "8. Walk-in", "Walk in" },
                                { "9. Others (employers, former pts, colleagues, friends)", "Others" },

                    };

            foreach (var referral in sourceOfReferral)
            {
                var key = referral.Key;
                var value = referral.Value;

                worksheet.Cell(referralRow, 1).Value = key;
                worksheet.Cell(referralRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(referralRow, 1).Style.Alignment.Indent = 2;

                for (int i = 1; i <= 6; i++)
                {
                    var count = generalAdmissions.Count(ga => ga.Referral.Equals(value, StringComparison.OrdinalIgnoreCase) && ga.Date.Month == i);
                    worksheet.Cell(referralRow, i + 1).Value = count == 0 ? "" : count;
                    worksheet.Cell(referralRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                worksheet.Cell(referralRow, 8).Value =
                    Enumerable.Range(1, 6).Sum(i => generalAdmissions.Count(ga => ga.Referral.Equals(value, StringComparison.OrdinalIgnoreCase) && ga.Date.Month == i));
                worksheet.Cell(referralRow, 8).Style.Font.Bold = true;
                worksheet.Cell(referralRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                for (int i = 7; i <= 12; i++)
                {
                    var count = generalAdmissions.Count(ga => ga.Referral.Equals(value, StringComparison.OrdinalIgnoreCase) && ga.Date.Month == i);
                    worksheet.Cell(referralRow, i + 2).Value = count == 0 ? "" : count;
                    worksheet.Cell(referralRow, i + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                worksheet.Cell(referralRow, 15).Value =
                    Enumerable.Range(7, 6).Sum(i => generalAdmissions.Count(ga => ga.Referral.Equals(value, StringComparison.OrdinalIgnoreCase) && ga.Date.Month == i));
                worksheet.Cell(referralRow, 15).Style.Font.Bold = true;
                worksheet.Cell(referralRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                referralRow++;
            }

            worksheet.Cell(referralRow, 1).Value = "TOTAL";
            worksheet.Cell(referralRow, 1).Style.Font.Bold = true;
            worksheet.Cell(referralRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int i = 1; i <= 6; i++)
            {
                var count = generalAdmissions.Count(ga => ga.Date.Month == i);
                worksheet.Cell(referralRow, i + 1).Value = count;
                worksheet.Cell(referralRow, i + 1).Style.Font.Bold = true;
                worksheet.Cell(referralRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(referralRow, 8).Value =
                Enumerable.Range(1, 6).Sum(i => generalAdmissions.Count(ga => ga.Date.Month == i));
            worksheet.Cell(referralRow, 8).Style.Font.Bold = true;
            worksheet.Cell(referralRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int i = 7; i <= 12; i++)
            {
                var count = generalAdmissions.Count(ga => ga.Date.Month == i);
                worksheet.Cell(referralRow, i + 2).Value = count;
                worksheet.Cell(referralRow, i + 2).Style.Font.Bold = true;
                worksheet.Cell(referralRow, i + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(referralRow, 15).Value =
                Enumerable.Range(7, 6).Sum(i => generalAdmissions.Count(ga => ga.Date.Month == i));
            worksheet.Cell(referralRow, 15).Style.Font.Bold = true;
            worksheet.Cell(referralRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // CASELOAD ACCORDING TO SERVICES
            int caseloadRow = referralRow + 1;

            worksheet.Cell(caseloadRow, 1).Value = "II. CASELOAD ACCORDING TO SERVICES";
            worksheet.Cell(caseloadRow, 1).Style.Font.Bold = true;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(caseloadRow, i + 1).Value = 0;
                    worksheet.Cell(caseloadRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(caseloadRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(caseloadRow, i + 1).Value = "";
                }
            }

            caseloadRow++;

            worksheet.Cell(caseloadRow, 1).Value = "1. During Census";
            worksheet.Cell(caseloadRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(caseloadRow, 1).Style.Alignment.Indent = 1;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(caseloadRow, i + 1).Value = 0;
                    worksheet.Cell(caseloadRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(caseloadRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(caseloadRow, i + 1).Value = "";
                }
            }

            caseloadRow++;

            worksheet.Cell(caseloadRow, 1).Value = "1.1 New Cases";
            worksheet.Cell(caseloadRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(caseloadRow, 1).Style.Alignment.Indent = 2;

            for (int i = 1; i <= 6; i++)
            {
                var count = generalAdmissions.Count(ga => !ga.isOld && ga.Date.Month == i);
                worksheet.Cell(caseloadRow, i + 1).Value = count == 0 ? "" : count;
                worksheet.Cell(caseloadRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(caseloadRow, 8).Value =
                Enumerable.Range(1, 6).Sum(i => generalAdmissions.Count(ga => !ga.isOld && ga.Date.Month == i));
            worksheet.Cell(caseloadRow, 8).Style.Font.Bold = true;
            worksheet.Cell(caseloadRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int i = 7; i <= 12; i++)
            {
                var count = generalAdmissions.Count(ga => !ga.isOld && ga.Date.Month == i);
                worksheet.Cell(caseloadRow, i + 2).Value = count == 0 ? "" : count;
                worksheet.Cell(caseloadRow, i + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(caseloadRow, 15).Value =
                Enumerable.Range(7, 6).Sum(i => generalAdmissions.Count(ga => !ga.isOld && ga.Date.Month == i));
            worksheet.Cell(caseloadRow, 15).Style.Font.Bold = true;
            worksheet.Cell(caseloadRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            caseloadRow++;

            worksheet.Cell(caseloadRow, 1).Value = "1.2 Old Cases";
            worksheet.Cell(caseloadRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(caseloadRow, 1).Style.Alignment.Indent = 2;

            for (int i = 1; i <= 6; i++)
            {
                var count = generalAdmissions.Count(ga => ga.isOld && ga.Date.Month == i);
                worksheet.Cell(caseloadRow, i + 1).Value = count == 0 ? "" : count;
                worksheet.Cell(caseloadRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(caseloadRow, 8).Value =
                Enumerable.Range(1, 6).Sum(i => generalAdmissions.Count(ga => ga.isOld && ga.Date.Month == i));
            worksheet.Cell(caseloadRow, 8).Style.Font.Bold = true;
            worksheet.Cell(caseloadRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int i = 7; i <= 12; i++)
            {
                var count = generalAdmissions.Count(ga => ga.isOld && ga.Date.Month == i);
                worksheet.Cell(caseloadRow, i + 2).Value = count == 0 ? "" : count;
                worksheet.Cell(caseloadRow, i + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(caseloadRow, 15).Value =
                Enumerable.Range(7, 6).Sum(i => generalAdmissions.Count(ga => ga.isOld && ga.Date.Month == i));
            worksheet.Cell(caseloadRow, 15).Style.Font.Bold = true;
            worksheet.Cell(caseloadRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            caseloadRow++;

            worksheet.Cell(caseloadRow, 1).Value = "2. Closed Summary";
            worksheet.Cell(caseloadRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(caseloadRow, 1).Style.Alignment.Indent = 1;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(caseloadRow, i + 1).Value = 0;
                    worksheet.Cell(caseloadRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(caseloadRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(caseloadRow, i + 1).Value = "";
                }
            }

            caseloadRow++;

            worksheet.Cell(caseloadRow, 1).Value = "3. Number of patients based on sectoral groupings mandated by law/policies:";
            worksheet.Cell(caseloadRow, 1).Style.Alignment.WrapText = true;
            worksheet.Cell(caseloadRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(caseloadRow, 1).Style.Alignment.Indent = 1;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(caseloadRow, i + 1).Value = 0;
                    worksheet.Cell(caseloadRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(caseloadRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(caseloadRow, i + 1).Value = "";
                }
            }

            caseloadRow++;

            worksheet.Cell(caseloadRow, 1).Value = "a. PWD";
            worksheet.Cell(caseloadRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(caseloadRow, 1).Style.Alignment.Indent = 2;

            for (int i = 1; i <= 6; i++)
            {
                var count = generalAdmissions.Count(ga => ga.isPWD && ga.Date.Month == i);
                worksheet.Cell(caseloadRow, i + 1).Value = count == 0 ? "" : count;
                worksheet.Cell(caseloadRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(caseloadRow, 8).Value =
                Enumerable.Range(1, 6).Sum(i => generalAdmissions.Count(ga => ga.isPWD && ga.Date.Month == i));
            worksheet.Cell(caseloadRow, 8).Style.Font.Bold = true;
            worksheet.Cell(caseloadRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int i = 7; i <= 12; i++)
            {
                var count = generalAdmissions.Count(ga => ga.isPWD && ga.Date.Month == i);
                worksheet.Cell(caseloadRow, i + 2).Value = count == 0 ? "" : count;
                worksheet.Cell(caseloadRow, i + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(caseloadRow, 15).Value =
                Enumerable.Range(7, 6).Sum(i => generalAdmissions.Count(ga => ga.isPWD && ga.Date.Month == i));
            worksheet.Cell(caseloadRow, 15).Style.Font.Bold = true;
            worksheet.Cell(caseloadRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            caseloadRow++;

            worksheet.Cell(caseloadRow, 1).Value = "b. Indigenous People";
            worksheet.Cell(caseloadRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(caseloadRow, 1).Style.Alignment.Indent = 2;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(caseloadRow, i + 1).Value = 0;
                    worksheet.Cell(caseloadRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(caseloadRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(caseloadRow, i + 1).Value = "";
                }
            }

            caseloadRow++;

            worksheet.Cell(caseloadRow, 1).Value = "c. Government Workers";
            worksheet.Cell(caseloadRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(caseloadRow, 1).Style.Alignment.Indent = 2;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(caseloadRow, i + 1).Value = 0;
                    worksheet.Cell(caseloadRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(caseloadRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(caseloadRow, i + 1).Value = "";
                }
            }

            // SERVICES
            int serviceRow = caseloadRow + 1;

            worksheet.Cell(serviceRow, 1).Value = "SERVICES";
            worksheet.Cell(serviceRow, 1).Style.Font.Bold = true;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "1. Planning, Screening and Eligibility Study / PSE";
            worksheet.Cell(serviceRow, 1).Style.Font.Bold = true;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "1.1 Socio Economic Classification";
            worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 1;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "1.2 Pre-admission Planning";
            worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 1;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "1.3 Information Services/Orientation";
            worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 1;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "2. Concrete and Referral Services";
            worksheet.Cell(serviceRow, 1).Style.Font.Bold = true;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "2.1 Provision of Discount";
            worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 1;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "2.2 Facilitating Referrals";
            worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 1;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "2.2.1 Outgoing Referrals for:";
            worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 1;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            var outgoingReferrals = new List<string>
            {
                "a. Medical Assistance",
                "b. Discount on Procedure/Hospital Expenses",
                "c. Transportation Fare",
                "d. Institutional Placement",
                "e. Temporary Shelter",
                "f. Funeral Assistance/Pauper's Burial",
                "g. Others specify (networking)"
            };

            foreach (var value in outgoingReferrals)
            {
                serviceRow++;

                worksheet.Cell(serviceRow, 1).Value = value;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 2;
                for (int i = 1; i <= headers.Length; i++)
                {
                    if (i == 7 || i == 14)
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = 0;
                        worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = "";
                    }
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "2.2.2 Incoming Referrals";
            worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 1;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "3. Psychosocial Counseling";
            worksheet.Cell(serviceRow, 1).Style.Font.Bold = true;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "Ten Leading Causes for Counselling";
            worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 1;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            var tenLeadingCauses = new List<string>
            {
                "1. Stress of the family",
                "2. Refusal of patient to take home",
                "3. Anxiety of health cost",
                "4. Marital problem",
                "5. Refusal of patient for treatment",
                "6. Unbecoming attitude due to postponement if surgery",
                "7. Emotional problem",
                "8. Neglected children",
                "9. Sexually abuse",
                "10. Adjustment problem"
            };

            foreach (var value in tenLeadingCauses)
            {
                serviceRow++;

                worksheet.Cell(serviceRow, 1).Value = value;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 2;
                for (int i = 1; i <= headers.Length; i++)
                {
                    if (i == 7 || i == 14)
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = 0;
                        worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = "";
                    }
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "3.2 Family Counselling";
            worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 1;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            var familyCounseling = new List<string>
            {
                "a. Social Worker",
                "b. Health Care Team"
            };

            foreach (var value in familyCounseling)
            {
                serviceRow++;

                worksheet.Cell(serviceRow, 1).Value = value;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 2;
                for (int i = 1; i <= headers.Length; i++)
                {
                    if (i == 7 || i == 14)
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = 0;
                        worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = "";
                    }
                }
            }

            var psychologicalCounseling = new List<string>
            {
                "3.3 Psychosocial Crisis Intervention",
                "3.4 Group Work/Per Session",
                "3.5 Patients/Watchers Education",
                "3.6 Mutual Support Group Session",
                "3.7 Advocacy Group"
            };

            foreach (var value in psychologicalCounseling)
            {
                serviceRow++;

                worksheet.Cell(serviceRow, 1).Value = value;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 1;
                for (int i = 1; i <= headers.Length; i++)
                {
                    if (i == 7 || i == 14)
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = 0;
                        worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = "";
                    }
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "4. Discharges Services";
            worksheet.Cell(serviceRow, 1).Style.Font.Bold = true;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            var dischargesServices = new List<string>
            {
                "a. Discharge Planning",
                "b. Facilitation of Discharge",
                "c. Pre-termination Counseling",
                "d. Home Conduction"
            };

            foreach (var value in dischargesServices)
            {
                serviceRow++;

                worksheet.Cell(serviceRow, 1).Value = value;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 1;
                for (int i = 1; i <= headers.Length; i++)
                {
                    if (i == 7 || i == 14)
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = 0;
                        worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = "";
                    }
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "5. Support Services";
            worksheet.Cell(serviceRow, 1).Style.Font.Bold = true;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "5.1 Ward Visitation";
            worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 1;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            var wardVisitation = new List<string>
            {
                "a. Individual",
                "b. Team"
            };

            foreach (var value in wardVisitation)
            {
                serviceRow++;

                worksheet.Cell(serviceRow, 1).Value = value;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 2;
                for (int i = 1; i <= headers.Length; i++)
                {
                    if (i == 7 || i == 14)
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = 0;
                        worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = "";
                    }
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "6. Case Conferences";
            worksheet.Cell(serviceRow, 1).Style.Font.Bold = true;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            var caseConferences = new List<string>
            {
                "a. Multi Disciplinary",
                "b. MSWD"
            };

            foreach (var value in caseConferences)
            {
                serviceRow++;

                worksheet.Cell(serviceRow, 1).Value = value;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 1;
                for (int i = 1; i <= headers.Length; i++)
                {
                    if (i == 7 || i == 14)
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = 0;
                        worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = "";
                    }
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "7. Follow-up Services";
            worksheet.Cell(serviceRow, 1).Style.Font.Bold = true;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            var followUpServices = new List<string>
            {
                "7.1 Home Visit",
                "7.2 Letters Sent",
                "7.3 Contact of Relatives by Telephone",
                "7.4 Contact of Relatives through Mass Media"
            };

            foreach (var value in followUpServices)
            {
                serviceRow++;

                worksheet.Cell(serviceRow, 1).Value = value;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 1;
                for (int i = 1; i <= headers.Length; i++)
                {
                    if (i == 7 || i == 14)
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = 0;
                        worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = "";
                    }
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "8. Coordination/Initiated by MSW";
            worksheet.Cell(serviceRow, 1).Style.Font.Bold = true;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            var coordinationsByMSW = new List<string>
            {
                "a. Physicians",
                "b. Nurses",
                "c. Pharmacist",
                "d. Nutritionist",
                "e. Other Staff",
                "f. Management"
            };

            foreach (var value in coordinationsByMSW)
            {
                serviceRow++;

                worksheet.Cell(serviceRow, 1).Value = value;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 1;
                for (int i = 1; i <= headers.Length; i++)
                {
                    if (i == 7 || i == 14)
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = 0;
                        worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = "";
                    }
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "9. Consultative and Advisory Services";
            worksheet.Cell(serviceRow, 1).Style.Font.Bold = true;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            var consultatives = new List<string>
            {
                "a. Physicians",
                "b. Office Staff",
                "c. Outside Hospital"
            };

            foreach (var value in consultatives)
            {
                serviceRow++;

                worksheet.Cell(serviceRow, 1).Value = value;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 1;
                for (int i = 1; i <= headers.Length; i++)
                {
                    if (i == 7 || i == 14)
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = 0;
                        worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = "";
                    }
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "10. Community Outreach";
            worksheet.Cell(serviceRow, 1).Style.Font.Bold = true;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "III. CASE MANAGEMENT SERVICES";
            worksheet.Cell(serviceRow, 1).Style.Font.Bold = true;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            var caseManagements = new List<string>
            {
                "1.1 Pre admission Counselling",
                "1.2 Intake Interview",
                "1.3 Collateral Interview",
                "1.4 Issuance of MSS Card",
                "1.5 Indicate classification in the chart (in pts only)",
                "1.6 Psychosocial Assessment",
                "1.7 Psychosocial Counselling",
                "1.8 Coordination w/ Multidiciplinary Team",
                "1.9 Completion of Intake Form",
                "1.10 Health Education",
                "1.11 Crisis Intervention",
                "1.12 Concrete Services"
            };

            foreach (var value in caseManagements)
            {
                serviceRow++;

                worksheet.Cell(serviceRow, 1).Value = value;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 1;
                for (int i = 1; i <= headers.Length; i++)
                {
                    if (i == 7 || i == 14)
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = 0;
                        worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = "";
                    }
                }
            }

            var concreteServices = new List<string>
            {
                "1.12.1 Facilitaion/Provision of Meds/Procedures",
                "1.12.2 Transportation Assistance (w/in MSS resources)",
                "1.12.3 Material Assistance (food, clothing)",
                "1.12.4 Financial Assistance (w/in MSS resources)"
            };

            foreach (var value in concreteServices)
            {
                serviceRow++;

                worksheet.Cell(serviceRow, 1).Value = value;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 2;
                for (int i = 1; i <= headers.Length; i++)
                {
                    if (i == 7 || i == 14)
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = 0;
                        worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = "";
                    }
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "1.13 Referral";
            worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 1;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            var referralServices = new List<string>
            {
                "1.13.1 Facilitating Incoming Referral",
                "1.13.2 Preparing the Referral",
                "1.13.3 Coordination w/ the Receiveing Agency"
            };

            foreach (var value in referralServices)
            {
                serviceRow++;

                worksheet.Cell(serviceRow, 1).Value = value;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 2;
                for (int i = 1; i <= headers.Length; i++)
                {
                    if (i == 7 || i == 14)
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = 0;
                        worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = "";
                    }
                }
            }

            var caseManagements2 = new List<string>
            {
                "1.14 Ward Rounds (no. of pts visited)",
                "1.15 Home Visitation",
                "1.16 Advocacy Role",
                "1.17 Education",
                "1.18 Therapeutic Social Work Services"
            };

            foreach (var value in caseManagements2)
            {
                serviceRow++;

                worksheet.Cell(serviceRow, 1).Value = value;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 1;
                for (int i = 1; i <= headers.Length; i++)
                {
                    if (i == 7 || i == 14)
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = 0;
                        worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = "";
                    }
                }
            }

            var therapeuticServices = new List<string>
            {
                "1.18.1 Abandoned",
                "1.18.2 Sexually Abused",
                "1.18.3 Neglected",
                "1.18.4 Battered"
            };

            foreach (var value in therapeuticServices)
            {
                serviceRow++;

                worksheet.Cell(serviceRow, 1).Value = value;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 2;
                for (int i = 1; i <= headers.Length; i++)
                {
                    if (i == 7 || i == 14)
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = 0;
                        worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = "";
                    }
                }
            }

            var caseManagements3 = new List<string>
            {
                "1.19 Protective Services",
                "1.20 Grief Work",
                "1.21 Behavioral Modification ",
                "1.22 Networking (meeting w/ other institution/grp org.)",
                "1.23 Politicians",
                "1.24 Coordination w/ Mass Media",
                "1.25 Consultaion/Advisory Services",
                "1.26 Attendance to Case Conferences Committee Meetings",
                "1.27 Attendance to Clinical Comittees"
            };

            foreach (var value in caseManagements3)
            {
                serviceRow++;

                worksheet.Cell(serviceRow, 1).Value = value;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 1;
                for (int i = 1; i <= headers.Length; i++)
                {
                    if (i == 7 || i == 14)
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = 0;
                        worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = "";
                    }
                }
            }

            var attendances = new List<string>
            {
                "1.27.1 Disharge Planning",
                "1.27.2 Facilitation of Discharge",
                "1.27.3 Home Conduction"
            };

            foreach (var value in attendances)
            {
                serviceRow++;

                worksheet.Cell(serviceRow, 1).Value = value;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 2;
                for (int i = 1; i <= headers.Length; i++)
                {
                    if (i == 7 || i == 14)
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = 0;
                        worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = "";
                    }
                }
            }

            var caseManagements4 = new List<string>
            {
                "1.28 Follow up Services",
                "1.29 Documentation"
            };

            foreach (var value in caseManagements4)
            {
                serviceRow++;

                worksheet.Cell(serviceRow, 1).Value = value;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 1;
                for (int i = 1; i <= headers.Length; i++)
                {
                    if (i == 7 || i == 14)
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = 0;
                        worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = "";
                    }
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "1.29.1 Profile";
            worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 2;
            for (int i = 1; i <= headers.Length; i++)
            {
                if (i == 7 || i == 14)
                {
                    worksheet.Cell(serviceRow, i + 1).Value = 0;
                    worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                else
                {
                    worksheet.Cell(serviceRow, i + 1).Value = "";
                }
            }

            serviceRow++;

            var progressQuery = context.ProgressNotes.AsQueryable();

            if (userID > 0)
            {
                progressQuery = progressQuery.Where(ga => ga.UserID == userID);
            }

            if (filterByMonth)
            {
                progressQuery = progressQuery.Where(ga => ga.Date.Month == parsedMonth.Month && ga.Date.Year == parsedMonth.Year);
            }

            var progressNotes = await progressQuery.ToListAsync();

            worksheet.Cell(serviceRow, 1).Value = "1.29.2 Progress Notes";
            worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 2;

            for (int i = 1; i <= 6; i++)
            {
                var count = progressNotes.Count(ga => ga.Date.Month == i);
                worksheet.Cell(serviceRow, i + 1).Value = count == 0 ? "" : count;
                worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(serviceRow, 8).Value =
                Enumerable.Range(1, 6).Sum(i => progressNotes.Count(ga => ga.Date.Month == i));
            worksheet.Cell(serviceRow, 8).Style.Font.Bold = true;
            worksheet.Cell(serviceRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int i = 7; i <= 12; i++)
            {
                var count = progressNotes.Count(ga => ga.Date.Month == i);
                worksheet.Cell(serviceRow, i + 2).Value = count == 0 ? "" : count;
                worksheet.Cell(serviceRow, i + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(serviceRow, 15).Value =
                Enumerable.Range(7, 6).Sum(i => progressNotes.Count(ga => ga.Date.Month == i));
            worksheet.Cell(serviceRow, 15).Style.Font.Bold = true;
            worksheet.Cell(serviceRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var documentations = new List<string>
            {
                "1.29.3 Groupwork Recording",
                "1.29.4 Social Case Study Report/Social Case Summary",
                "1.29.5 Home Visit Report"
            };

            foreach (var value in documentations)
            {
                serviceRow++;

                worksheet.Cell(serviceRow, 1).Value = value;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 2;
                for (int i = 1; i <= headers.Length; i++)
                {
                    if (i == 7 || i == 14)
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = 0;
                        worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = "";
                    }
                }
            }

            var caseManagements5 = new List<string>
            {
                "1.30 Palliative Care",
                "1.31 Facilitation of Unclaimed Cadaver",
                "1.32 Post Discharge Services",
                "1.33 Follow up Services through text/phone",
                "1.34 Follow up Treatment Plans",
                "1.35 Follow up of Rehabilitation Plans",
                "1.36 Rehabilitation Services"
            };

            foreach (var value in caseManagements5)
            {
                serviceRow++;

                worksheet.Cell(serviceRow, 1).Value = value;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 1;
                for (int i = 1; i <= headers.Length; i++)
                {
                    if (i == 7 || i == 14)
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = 0;
                        worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = "";
                    }
                }
            }

            var rehabilitationServices = new List<string>
            {
                "1.36.1 Skills Training",
                "1.36.2 Job Placement",
                "1.36.3 Capital Assistance"
            };

            foreach (var value in rehabilitationServices)
            {
                serviceRow++;

                worksheet.Cell(serviceRow, 1).Value = value;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 2;
                for (int i = 1; i <= headers.Length; i++)
                {
                    if (i == 7 || i == 14)
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = 0;
                        worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = "";
                    }
                }
            }

            var caseManagements6 = new List<string>
            {
                "1.37 MSWD Fund Raising Activity",
                "1.38 Hospital Activity",
                "1.39 Linkage w/ Donors"
            };

            foreach (var value in caseManagements6)
            {
                serviceRow++;

                worksheet.Cell(serviceRow, 1).Value = value;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(serviceRow, 1).Style.Alignment.Indent = 1;
                for (int i = 1; i <= headers.Length; i++)
                {
                    if (i == 7 || i == 14)
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = 0;
                        worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                    {
                        worksheet.Cell(serviceRow, i + 1).Value = "";
                    }
                }
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "TOTAL";
            worksheet.Cell(serviceRow, 1).Style.Font.Bold = true;
            worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int i = 1; i <= 6; i++)
            {
                var count = generalAdmissions.Count(ga => ga.Date.Month == i) + progressNotes.Count(ga => ga.Date.Month == i);
                worksheet.Cell(serviceRow, i + 1).Value = count;
                worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(serviceRow, 8).Value =
                Enumerable.Range(1, 6).Sum(i => generalAdmissions.Count(ga => ga.Date.Month == i) + progressNotes.Count(ga => ga.Date.Month == i));
            worksheet.Cell(serviceRow, 8).Style.Font.Bold = true;
            worksheet.Cell(serviceRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int i = 7; i <= 12; i++)
            {
                var count = generalAdmissions.Count(ga => ga.Date.Month == i) + progressNotes.Count(ga => ga.Date.Month == i);
                worksheet.Cell(serviceRow, i + 2).Value = count;
                worksheet.Cell(serviceRow, i + 2).Style.Font.Bold = true;
                worksheet.Cell(serviceRow, i + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(serviceRow, 15).Value =
                Enumerable.Range(7, 6).Sum(i => generalAdmissions.Count(ga => ga.Date.Month == i) + progressNotes.Count(ga => ga.Date.Month == i));
            worksheet.Cell(serviceRow, 15).Style.Font.Bold = true;
            worksheet.Cell(serviceRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ExcelReportStyler.ApplyWorksheetDesign(worksheet, new List<int> { 1, 2, 3 }, new List<int> { headerRow }, new List<int> { referralRow, serviceRow }, serviceRow, User.FindFirst(ClaimTypes.Name).Value, false, false, true);

            // Autofit for better presentation
            worksheet.Column(1).AdjustToContents();

            await using (var stream = new MemoryStream())
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
