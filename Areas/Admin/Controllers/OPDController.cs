using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using DocumentFormat.OpenXml.Spreadsheet;
using LittleArkFoundation.Areas.Admin.Data;
using LittleArkFoundation.Areas.Admin.Models.Discharges;
using LittleArkFoundation.Areas.Admin.Models.Form;
using LittleArkFoundation.Areas.Admin.Models.OPD;
using LittleArkFoundation.Areas.Admin.Services.Reports;
using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace LittleArkFoundation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [HasPermission("ManageOPD")]
    public class OPDController : Controller
    {
        private readonly ConnectionService _connectionService;
        public OPDController(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<IActionResult> Index(string? sortToggle, string? sortByMonth, int page = 1, int pageSize = 20)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            string sortToggleValue = sortToggle ?? "All";
            ViewBag.sortToggle = sortToggleValue;

            var query = context.OPD.AsQueryable();
            
            if (sortToggleValue == "Admitted")
            {
                // Fetch only admitted patients
                query = context.OPD.Where(opd => opd.IsAdmitted);
            }
            else if (sortToggleValue == "Not Admitted")
            {
                // Fetch only non-admitted patients
                query = context.OPD.Where(opd => !opd.IsAdmitted);
            }

            if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime month))
            {
                query = query.Where(opd => opd.Date.Month == month.Month && opd.Date.Year == month.Year);
                ViewBag.sortByMonth = month.ToString("yyyy-MM");
            }

            // Pagination
            var totalCount = await query.CountAsync();
            var opdList = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var scoredList = new List<(OPDModel opd, Dictionary<string, int> scores, bool isEligible)>();
            var _scoreService = new OPDScoringService(connectionString);
            foreach (var opd in opdList)
            {
                var scores = await _scoreService.GetWeightedScoresAsync(opd);
                var isEligible = await _scoreService.IsEligibleForAdmissionAsync(scores.Values.Sum());
                scoredList.Add((opd, scores, isEligible));
            }

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            var viewModel = new OPDViewModel
            {
                OPDList = opdList,
                OPDScoringList = scoredList,
                Users = users,
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
                return RedirectToAction("Index", new {sortToggle = sortToggleValue });
            }

            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var searchWords = searchString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            IQueryable<OPDModel> query = context.OPD.AsQueryable();

            if (sortToggleValue == "Admitted")
            {
                query = query.Where(u => u.IsAdmitted);
            }
            else if (sortToggleValue == "Not Admitted")
            {
                query = query.Where(u => !u.IsAdmitted);
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
            var opdList = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var scoredList = new List<(OPDModel opd, Dictionary<string, int> scores, bool isEligible)>();
            var _scoreService = new OPDScoringService(connectionString);
            foreach (var opd in opdList)
            {
                var scores = await _scoreService.GetWeightedScoresAsync(opd);
                var isEligible = await _scoreService.IsEligibleForAdmissionAsync(scores.Values.Sum());
                scoredList.Add((opd, scores, isEligible));
            }

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            var viewModel = new OPDViewModel
            {
                OPDList = opdList,
                OPDScoringList = scoredList,
                Users = users,
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

            IQueryable<OPDModel> query = context.OPD.AsQueryable();

            if (!string.IsNullOrEmpty(sortByUserID))
            {
                query = query.Where(opd => opd.UserID == int.Parse(sortByUserID));
                var user = await context.Users.FindAsync(int.Parse(sortByUserID));
                ViewBag.sortBy = user.Username;
                ViewBag.sortByUserID = user.UserID.ToString();
            }

            if (sortToggleValue == "Admitted")
            {
                query = query.Where(opd => opd.IsAdmitted);
            }
            else if (sortToggleValue == "Not Admitted")
            {
                query = query.Where(opd => !opd.IsAdmitted);
            }

            if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime month))
            {
                query = query.Where(opd => opd.Date.Month == month.Month && opd.Date.Year == month.Year);
                ViewBag.sortByMonth = month.ToString("yyyy-MM");
            }

            // Pagination
            var totalCount = await query.CountAsync();
            var opdList = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var scoredList = new List<(OPDModel opd, Dictionary<string, int> scores, bool isEligible)>();
            var _scoreService = new OPDScoringService(connectionString);
            foreach (var opd in opdList)
            {
                var scores = await _scoreService.GetWeightedScoresAsync(opd);
                var isEligible = await _scoreService.IsEligibleForAdmissionAsync(scores.Values.Sum());
                scoredList.Add((opd, scores, isEligible));
            }

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            var viewModel = new OPDViewModel
            {
                OPDList = opdList,
                OPDScoringList = scoredList,
                Users = users,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
            };
            
            return View("Index", viewModel);
        }

        public async Task<IActionResult> SortByOPDAssistedAndReports(string sortByUserID, string? sortByMonth, string? viewName, int page = 1, int pageSize = 20)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            IQueryable<OPDModel> query = context.OPD.AsQueryable();

            if (!string.IsNullOrEmpty(sortByUserID))
            {
                query = query.Where(opd => opd.UserID == int.Parse(sortByUserID));
                var user = await context.Users.FindAsync(int.Parse(sortByUserID));
                ViewBag.sortBy = user.Username;
                ViewBag.sortByUserID = user.UserID.ToString();
            }

            if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime month))
            {
                query = query.Where(opd => opd.Date.Month == month.Month && opd.Date.Year == month.Year);
                ViewBag.sortByMonth = month.ToString("yyyy-MM");
            }

            // Pagination
            var totalCount = await query.CountAsync();
            var opdList = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            var viewModel = new OPDViewModel
            {
                OPDList = opdList,
                Users = users,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
            };

            return View(viewName, viewModel);
        }

        public async Task<IActionResult> SortByGLReceivedAndReports(string sortByUserID, string? sortByMonth, string? viewName, int page = 1, int pageSize = 20)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            IQueryable<OPDModel> query = context.OPD.AsQueryable();

            if (!string.IsNullOrEmpty(sortByUserID))
            {
                query = query.Where(opd => opd.UserID == int.Parse(sortByUserID));
                var user = await context.Users.FindAsync(int.Parse(sortByUserID));
                ViewBag.sortBy = user.Username;
                ViewBag.sortByUserID = user.UserID.ToString();
            }

            if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime month))
            {
                query = query.Where(opd => opd.Date.Month == month.Month && opd.Date.Year == month.Year);
                ViewBag.sortByMonth = month.ToString("yyyy-MM");
            }

            // Pagination
            var totalCount = await query.CountAsync();
            var opdList = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            var viewModel = new OPDViewModel
            {
                OPDList = opdList,
                Users = users,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
            };

            return View(viewName, viewModel);
        }

        public async Task<IActionResult> SortByStatistics(string sortByUserID, string? sortByMonth, string? viewName)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            IQueryable<OPDModel> query = context.OPD.AsQueryable();

            if (!string.IsNullOrEmpty(sortByUserID))
            {
                query = query.Where(opd => opd.UserID == int.Parse(sortByUserID));
                var user = await context.Users.FindAsync(int.Parse(sortByUserID));
                ViewBag.sortBy = user.Username;
                ViewBag.sortByUserID = user.UserID.ToString();
            }

            if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime month))
            {
                query = query.Where(opd => opd.Date.Month == month.Month && opd.Date.Year == month.Year);
                ViewBag.sortByMonth = month.ToString("yyyy-MM");
            }

            var opdList = await query.ToListAsync();

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            var viewModel = new OPDViewModel
            {
                OPDList = opdList,
                Users = users
            };

            return View(viewName, viewModel);
        }

        [HasPermission("CreateOPD")]
        public async Task<IActionResult> Create()
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.UserID == int.Parse(userIdClaim.Value));

            var viewModel = new OPDViewModel
            {
                User = user
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission("CreateOPD")]
        public async Task<IActionResult> Create(OPDViewModel viewModel)
        {
            try
            {
                string connectionString = _connectionService.GetCurrentConnectionString();
                await using var context = new ApplicationDbContext(connectionString);

                await context.OPD.AddAsync(viewModel.OPD);
                await context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Successfully created new OPD";
                LoggingService.LogInformation($"OPD Patient creation successful. Created OPD Id: {viewModel.OPD.Id}. Created by UserID: {User.FindFirst(ClaimTypes.NameIdentifier).Value}, DateTime: {DateTime.Now}");
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

        [HasPermission("EditOPD")]
        public async Task<IActionResult> Edit(int id)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);
            var opd = await context.OPD.FindAsync(id);
            if (opd == null)
            {
                TempData["ErrorMessage"] = "OPD not found.";
                return RedirectToAction("Index");
            }
            var viewModel = new OPDViewModel
            {
                OPD = opd
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission("EditOPD")]
        public async Task<IActionResult> Edit(OPDViewModel viewModel)
        {
            try
            {
                string connectionString = _connectionService.GetCurrentConnectionString();
                await using var context = new ApplicationDbContext(connectionString);
                context.OPD.Update(viewModel.OPD);
                await context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully edited/updated OPD Id: {viewModel.OPD.Id}";
                LoggingService.LogInformation($"OPD Patient edited/updated successful. Updated OPD Id: {viewModel.OPD.Id}. Updated by UserID: {User.FindFirst(ClaimTypes.NameIdentifier).Value}, DateTime: {DateTime.Now}");
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

        [HasPermission("DeleteOPD")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                string connectionString = _connectionService.GetCurrentConnectionString();
                await using var context = new ApplicationDbContext(connectionString);
                var opd = await context.OPD.FindAsync(id);
                if (opd == null)
                {
                    TempData["ErrorMessage"] = "OPD not found.";
                    return RedirectToAction("Index");
                }
                context.OPD.Remove(opd);
                await context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully deleted OPD Id: {id}";
                LoggingService.LogInformation($"OPD Patient deleted successful. Deleted OPD Id: {id}. Deleted by UserID: {User.FindFirst(ClaimTypes.NameIdentifier).Value}, DateTime: {DateTime.Now}");
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

        public async Task<IActionResult> ExportLogsheetToExcel(int userID, string? month)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            // Parse the month input if provided
            bool filterByMonth = DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedMonth);

            IQueryable<OPDModel> query = context.OPD;

            if (userID > 0)
            {
                query = query.Where(opd => opd.UserID == userID);
            }

            if (filterByMonth)
            {
                query = query.Where(opd => opd.Date.Month == parsedMonth.Month && opd.Date.Year == parsedMonth.Year);
            }

            var opdList = await query.ToListAsync();

            if (opdList == null || !opdList.Any())
            {
                TempData["ErrorMessage"] = "No OPD records found for selected filters.";
                return RedirectToAction("Index");
            }

            // File name generation
            string mswName = userID > 0 ? opdList.First().MSW : "All MSW";
            string monthLabel = filterByMonth ? parsedMonth.ToString("MMMM_yyyy") : opdList.First().Date.Year.ToString();
            string fileName = $"OPD_Logsheet_{monthLabel}_{mswName}";

            // Sanitize sheet name (for Excel)
            string safeSheetName = Regex.Replace(fileName, @"[\[\]\*\?/\\:]", "_");
            if (safeSheetName.Length > 31)
                safeSheetName = safeSheetName.Substring(0, 31);

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(safeSheetName);

            // HEADERS

            var headers = new[]
            {
                "Date", "No", "Old/New", "Class", "Name of Patient",
                "Age", "G", "PWD", "Diagnosis", "Complete Address",
                "Source of Referral", "Name of Parents", "Occupation",
                "Monthly Income", "No. of Children", "Assistance Needed",
                "Amount", "PT's Share", "Amount Extended", "Resources",
                "Proponent of GL", "Amount of Received GL", "MSW", "Category"
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
            cell2.Value = "OPD LOGSHEET";
            cell2.Style.Font.Bold = true;
            cell2.Style.Font.FontSize = 12;
            cell2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range(2, 1, 2, headers.Count()).Merge();

            // Column 3
            var cell3 = worksheet.Cell(3, 1);
            cell3.Value = $"{monthLabel} OPD";
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
            foreach (var opd in opdList)
            {
                worksheet.Cell(dataRow, 1).Value = opd.Date.ToShortDateString();
                worksheet.Cell(dataRow, 2).Value = opd.Id;
                worksheet.Cell(dataRow, 3).Value = opd.IsOld ? "Old" : "New";
                worksheet.Cell(dataRow, 4).Value = opd.Class;
                worksheet.Cell(dataRow, 5).Value = $"{opd.LastName}, {opd.FirstName} {opd.MiddleName}";
                worksheet.Cell(dataRow, 6).Value = opd.Age;
                worksheet.Cell(dataRow, 7).Value = opd.Gender;
                worksheet.Cell(dataRow, 8).Value = opd.IsPWD ? "Yes" : "No";
                worksheet.Cell(dataRow, 9).Value = opd.Diagnosis;
                worksheet.Cell(dataRow, 10).Value = opd.Address;
                worksheet.Cell(dataRow, 11).Value = opd.SourceOfReferral;
                worksheet.Cell(dataRow, 12).Value = $"{opd.MotherLastName} / " +
                                                     $"{opd.FatherLastName}";
                worksheet.Cell(dataRow, 13).Value = $"{opd.MotherOccupation} / " + 
                                                     $"{opd.FatherOccupation}";
                worksheet.Cell(dataRow, 14).Value = opd.MonthlyIncome;
                worksheet.Cell(dataRow, 15).Value = opd.NoOfChildren;
                worksheet.Cell(dataRow, 16).Value = opd.AssistanceNeeded;
                worksheet.Cell(dataRow, 17).Value = opd.Amount;
                worksheet.Cell(dataRow, 18).Value = opd.PtShare;
                worksheet.Cell(dataRow, 19).Value = opd.AmountExtended;
                worksheet.Cell(dataRow, 20).Value = opd.Resources;
                worksheet.Cell(dataRow, 21).Value = opd.GLProponent;
                worksheet.Cell(dataRow, 22).Value = opd.GLAmountReceived;
                worksheet.Cell(dataRow, 23).Value = opd.MSW;
                worksheet.Cell(dataRow, 24).Value = opd.Category;

                dataRow++;
            }

            ExcelReportStyler.ApplyWorksheetDesign(worksheet, new List<int> { 1, 2, 3 }, new List<int> { headerRow }, new List<int> { dataRow }, dataRow, User.FindFirst(ClaimTypes.Name).Value, false, true);

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

            var opdList = await context.OPD.ToListAsync();
            if (opdList == null || !opdList.Any())
            {
                TempData["ErrorMessage"] = "No OPD records found.";
                return RedirectToAction("Index");
            }

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            var viewModel = new OPDViewModel
            {
                OPDList = opdList,
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

            IQueryable<OPDModel> query = context.OPD;

            if (userID > 0)
            {
                query = query.Where(opd => opd.UserID == userID);
            }

            if (filterByMonth)
            {
                query = query.Where(opd => opd.Date.Month == parsedMonth.Month && opd.Date.Year == parsedMonth.Year);
            }

            var opdList = await query.ToListAsync();

            if (opdList == null || !opdList.Any())
            {
                TempData["ErrorMessage"] = "No OPD records found for selected filters.";
                return RedirectToAction("Reports");
            }

            // File name generation
            string mswName = userID > 0 ? opdList.First().MSW : "All MSW";
            string monthLabel = filterByMonth ? parsedMonth.ToString("MMMM_yyyy") : opdList.First().Date.Year.ToString();
            string fileName = $"OPD_Reports_{monthLabel}";

            // Sanitize sheet name (for Excel)
            string safeSheetName = Regex.Replace(fileName, @"[\[\]\*\?/\\:]", "_");
            if (safeSheetName.Length > 31)
                safeSheetName = safeSheetName.Substring(0, 31);

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(safeSheetName);

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            // HEADERS
            // COUNTA OF DATE PROCESSED BY MSW
            worksheet.Cell(4, 1).Value = "COUNTA OF DATE PROCESSED BY MSW";
            worksheet.Cell(5, 1).Value = "Date Processed by MSW";

            int dateCol = 2;
            foreach (var user in users)
            {
                worksheet.Cell(5, dateCol).Value = user.Username;
                dateCol++;
            }

            worksheet.Cell(5, dateCol).Value = "Grand Total";

            // Prepare data grouped by ProcessedDate
            var groupedOPD = opdList
                .GroupBy(d => d.Date)
                .OrderBy(g => g.Key)
                .ToList();

            int dateRow = 6;
            foreach (var group in groupedOPD)
            {
                worksheet.Cell(dateRow, 1).Value = group.Key.ToShortDateString();

                int currentCol = 2;
                foreach (var user in users)
                {
                    int count = group.Count(d => d.UserID == user.UserID);
                    worksheet.Cell(dateRow, currentCol).Value = count;
                    currentCol++;
                }

                worksheet.Cell(dateRow, currentCol).Value = group.Count(); // Grand total
                dateRow++;
            }

            // After rows
            int totalDateRow = dateRow;

            worksheet.Cell(totalDateRow, 1).Value = "Total";

            // For each user column, calculate total opd across all dates
            int userCol = 2;
            foreach (var user in users)
            {
                int userTotal = opdList.Count(d => d.UserID == user.UserID);
                worksheet.Cell(totalDateRow, userCol).Value = userTotal;
                userCol++;
            }

            // Grand total: total number of opd records
            worksheet.Cell(totalDateRow, userCol).Value = opdList.Count;
            worksheet.Row(totalDateRow).Style.Font.Bold = true;

            // HEADERS
            // COUNTA OF CLASS
            int classRowStart = totalDateRow + 2;

            worksheet.Cell(classRowStart, 1).Value = "COUNTA OF CLASS";
            worksheet.Cell(classRowStart + 1, 1).Value = "Class";

            int classCol = 2;
            foreach (var user in users)
            {
                worksheet.Cell(classRowStart + 1, classCol).Value = user.Username;
                classCol++;
            }

            worksheet.Cell(classRowStart + 1, classCol).Value = "Grand Total";

            // Prepare data grouped by Class
            var classes = new List<string>
            {
                "A", "B", "C1", "C2", "C3", "D"
            };
            var groupedClass = opdList
                .Where(d => !string.IsNullOrEmpty(d.Class))
                .GroupBy(d => d.Class)
                .ToDictionary(g => g.Key, g => g.ToList());

            int classRowEnd = classRowStart + 2;
            foreach (var cls in classes)
            {
                worksheet.Cell(classRowEnd, 1).Value = cls;
                int currentCol = 2;
                foreach (var user in users)
                {
                    worksheet.Cell(classRowEnd, currentCol).Value = groupedClass.ContainsKey(cls) 
                        ? groupedClass[cls].Count(d => d.UserID == user.UserID) 
                        : 0;
                    currentCol++;
                }

                worksheet.Cell(classRowEnd, currentCol).Value = groupedClass.ContainsKey(cls) 
                    ? groupedClass[cls].Count() 
                    : 0; // Grand total for class
                classRowEnd++;
            }

            // After class rows
            int totalClassRow = classRowEnd;
            worksheet.Cell(totalClassRow, 1).Value = "Total";

            int classTotalCol = 2;
            foreach (var user in users)
            {
                int classTotal = opdList.Count(d => d.UserID == user.UserID && classes.Contains(d.Class));
                worksheet.Cell(totalClassRow, classTotalCol).Value = classTotal;
                classTotalCol++;
            }

            // Grand total for classes
            worksheet.Cell(totalClassRow, classTotalCol).Value = opdList.Count(d => classes.Contains(d.Class));
            worksheet.Row(totalClassRow).Style.Font.Bold = true;

            // HEADERS
            // COUNTA OF CLASS BY GENDER
            int genderRowStart = totalClassRow + 2;

            worksheet.Cell(genderRowStart, 1).Value = "COUNTA OF CLASS BY GENDER";
            worksheet.Cell(genderRowStart + 1, 1).Value = "Class";
            worksheet.Cell(genderRowStart + 1, 2).Value = "F";
            worksheet.Cell(genderRowStart + 1, 3).Value = "M";
            worksheet.Cell(genderRowStart + 1, 4).Value = "Grand Total";

            int genderRowEnd = genderRowStart + 2;
            foreach (var cls in classes)
            {
                worksheet.Cell(genderRowEnd, 1).Value = cls;

                int femaleCount = opdList.Count(d => d.Class == cls && d.Gender == "Female");
                int maleCount = opdList.Count(d => d.Class == cls && d.Gender == "Male");

                worksheet.Cell(genderRowEnd, 2).Value = femaleCount;
                worksheet.Cell(genderRowEnd, 3).Value = maleCount;
                worksheet.Cell(genderRowEnd, 4).Value = femaleCount + maleCount; // Grand total for class

                genderRowEnd++;
            }

            // After gender rows
            int totalGenderRow = genderRowEnd;

            worksheet.Cell(totalGenderRow, 1).Value = "Total";
            worksheet.Cell(totalGenderRow, 2).Value = opdList.Count(d => d.Gender == "Female");
            worksheet.Cell(totalGenderRow, 3).Value = opdList.Count(d => d.Gender == "Male");
            worksheet.Cell(totalGenderRow, 4).Value = opdList.Count(d => d.Gender == "Female") + opdList.Count(d => d.Gender == "Male");
            worksheet.Row(totalGenderRow).Style.Font.Bold = true;


            // HEADERS
            // COUNTA OF OLD/NEW
            int oldNewRowStart = totalGenderRow + 2;

            worksheet.Cell(oldNewRowStart, 1).Value = "COUNTA OF OLD/NEW";
            worksheet.Cell(oldNewRowStart + 1, 1).Value = "MSW";
            worksheet.Cell(oldNewRowStart + 1, 2).Value = "Old";
            worksheet.Cell(oldNewRowStart + 1, 3).Value = "New";
            worksheet.Cell(oldNewRowStart + 1, 4).Value = "Grand Total";

            int oldNewRowEnd = oldNewRowStart + 2;
            foreach (var user in users)
            {
                worksheet.Cell(oldNewRowEnd, 1).Value = user.Username;
                int oldCount = opdList.Count(d => d.UserID == user.UserID && d.IsOld);
                int newCount = opdList.Count(d => d.UserID == user.UserID && !d.IsOld);
                worksheet.Cell(oldNewRowEnd, 2).Value = oldCount;
                worksheet.Cell(oldNewRowEnd, 3).Value = newCount;
                worksheet.Cell(oldNewRowEnd, 4).Value = oldCount + newCount; // Grand total for user
                oldNewRowEnd++;
            }

            // After old/new rows
            int totalOldNewRow = oldNewRowEnd;

            worksheet.Cell(totalOldNewRow, 1).Value = "Total";
            int oldTotal = opdList.Count(d => d.IsOld);
            int newTotal = opdList.Count(d => !d.IsOld);
            worksheet.Cell(totalOldNewRow, 2).Value = oldTotal;
            worksheet.Cell(totalOldNewRow, 3).Value = newTotal;
            worksheet.Cell(totalOldNewRow, 4).Value = oldTotal + newTotal; // Grand total for old/new
            worksheet.Row(totalOldNewRow).Style.Font.Bold = true;

            // HEADERS
            // COUNTA OF PWD
            int pwdRowStart = totalOldNewRow + 2;

            worksheet.Cell(pwdRowStart, 1).Value = "COUNTA OF PWD";
            worksheet.Cell(pwdRowStart + 1, 1).Value = "MSW";
            worksheet.Cell(pwdRowStart + 1, 2).Value = "PWD";
            worksheet.Cell(pwdRowStart + 1, 3).Value = "Non-PWD";
            worksheet.Cell(pwdRowStart + 1, 4).Value = "Grand Total";

            int pwdRowEnd = pwdRowStart + 2;
            foreach (var user in users)
            {
                worksheet.Cell(pwdRowEnd, 1).Value = user.Username;
                int pwdCount = opdList.Count(d => d.UserID == user.UserID && d.IsPWD);
                int nonPwdCount = opdList.Count(d => d.UserID == user.UserID && !d.IsPWD);
                worksheet.Cell(pwdRowEnd, 2).Value = pwdCount;
                worksheet.Cell(pwdRowEnd, 3).Value = nonPwdCount;
                worksheet.Cell(pwdRowEnd, 4).Value = pwdCount + nonPwdCount; // Grand total for user
                pwdRowEnd++;
            }

            // After PWD rows
            int totalPwdRow = pwdRowEnd;

            worksheet.Cell(totalPwdRow, 1).Value = "Total";
            int pwdTotal = opdList.Count(d => d.IsPWD);
            int nonPwdTotal = opdList.Count(d => !d.IsPWD);
            worksheet.Cell(totalPwdRow, 2).Value = pwdTotal;
            worksheet.Cell(totalPwdRow, 3).Value = nonPwdTotal;
            worksheet.Cell(totalPwdRow, 4).Value = pwdTotal + nonPwdTotal; // Grand total for PWD

            // HEADERS
            // COUNTA OF REFERRAL PROCESSED BY MSW
            int referralRowStart = totalPwdRow + 2;

            worksheet.Cell(referralRowStart, 1).Value = "COUNTA OF REFERRAL BY MSW";
            worksheet.Cell(referralRowStart + 1, 1).Value = "Referral";

            int referralColIndex = 2;
            foreach (var user in users)
            {
                worksheet.Cell(referralRowStart + 1, referralColIndex).Value = user.Username;
                referralColIndex++;
            }

            worksheet.Cell(referralRowStart + 1, referralColIndex).Value = "Grand Total";

            // Prepare data grouped by ProcessedDate
            var groupedReferral = opdList
                .GroupBy(d => d.SourceOfReferral)
                .OrderBy(g => g.Key)
                .ToList();

            int referralDataRowIndex = referralRowStart + 2;
            foreach (var group in groupedReferral)
            {
                worksheet.Cell(referralDataRowIndex, 1).Value = group.Key;

                int colIndex = 2;
                foreach (var user in users)
                {
                    var count = group.Count(d => d.UserID == user.UserID);
                    worksheet.Cell(referralDataRowIndex, colIndex).Value = count;
                    colIndex++;
                }

                worksheet.Cell(referralDataRowIndex, colIndex).Value = group.Count(); // Grand Total
                referralDataRowIndex++;
            }

            int totalReferralDataRowIndex = referralDataRowIndex;
            worksheet.Cell(totalReferralDataRowIndex, 1).Value = "Total";

            int totalReferralDataColIndex = 2;
            foreach (var user in users)
            {
                var totalCount = groupedReferral.Sum(g => g.Count(d => d.UserID == user.UserID));
                worksheet.Cell(totalReferralDataRowIndex, totalReferralDataColIndex).Value = totalCount;
                totalReferralDataColIndex++;
            }

            worksheet.Cell(totalReferralDataRowIndex, totalReferralDataColIndex).Value = groupedReferral.Sum(g => g.Count()); // Grand Total
            worksheet.Row(totalReferralDataRowIndex).Style.Font.Bold = true;

            // Column 1
            var cell2 = worksheet.Cell(1, 1);
            cell2.Value = "OPD Reports";
            cell2.Style.Font.Bold = true;
            cell2.Style.Font.FontSize = 12;
            cell2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell2.Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Range(1, 1, 1, worksheet.LastColumnUsed().ColumnNumber()).Merge();

            // Column 2
            var cell3 = worksheet.Cell(2, 1);
            cell3.Value = $"{monthLabel} OPD";
            cell3.Style.Font.Bold = true;
            cell3.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell3.Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Range(2, 1, 2, worksheet.LastColumnUsed().ColumnNumber()).Merge();

            // Set header row style 
            var rowsList = new List<int>
            {
                4, classRowStart, genderRowStart, pwdRowStart, referralRowStart, oldNewRowStart
            };

            var headerRowsList = new List<int>
            {
                5, classRowStart + 1, genderRowStart + 1, pwdRowStart + 1, referralRowStart + 1, oldNewRowStart + 1
            };

            var totalRowsList = new List<int>
            {
                totalDateRow, totalClassRow, totalGenderRow, totalPwdRow, totalReferralDataRowIndex, totalOldNewRow
            };

            var userNameClaim = User.FindFirst(ClaimTypes.Name).Value;

            ExcelReportStyler.ApplyWorksheetDesign(worksheet, rowsList, headerRowsList, totalRowsList, totalReferralDataRowIndex, userNameClaim, true);

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

        public async Task<IActionResult> OPDAssisted(string? sortByMonth, int page = 1, int pageSize = 20)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var query = context.OPD.AsQueryable();

            if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime month))
            {
                query = query.Where(opd => opd.Date.Month == month.Month && opd.Date.Year == month.Year);
                ViewBag.sortByMonth = month.ToString("yyyy-MM");
            }

            // Pagination
            var totalCount = await query.CountAsync();
            var opdList = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (opdList == null || !opdList.Any())
            {
                TempData["ErrorMessage"] = "No OPD records found.";
                return RedirectToAction("Index");
            }

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            var viewModel = new OPDViewModel
            {
                OPDList = opdList,
                Users = users,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
            };

            return View(viewModel);
        }

        public async Task<IActionResult> GLReceived(string? sortByMonth, int page = 1, int pageSize = 20)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var query = context.OPD.AsQueryable();

            if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime month))
            {
                query = query.Where(opd => opd.Date.Month == month.Month && opd.Date.Year == month.Year);
                ViewBag.sortByMonth = month.ToString("yyyy-MM");
            }

            // Pagination
            var totalCount = await query.CountAsync();
            var opdList = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (opdList == null || !opdList.Any())
            {
                TempData["ErrorMessage"] = "No OPD records found.";
                return RedirectToAction("Index");
            }

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            var viewModel = new OPDViewModel
            {
                OPDList = opdList,
                Users = users,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ExportOPDAssistedToExcel(int userID, string? month)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            // Parse the month input if provided
            bool filterByMonth = DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedMonth);

            IQueryable<OPDModel> query = context.OPD;

            if (userID > 0)
            {
                query = query.Where(opd => opd.UserID == userID);
            }

            if (filterByMonth)
            {
                query = query.Where(opd => opd.Date.Month == parsedMonth.Month && opd.Date.Year == parsedMonth.Year);
            }

            var opdList = await query.ToListAsync();

            if (opdList == null || !opdList.Any())
            {
                TempData["ErrorMessage"] = "No OPD records found for selected filters.";
                return RedirectToAction("OPDAssisted");
            }

            // File name generation
            string mswName = userID > 0 ? opdList.First().MSW : "All MSW";
            string monthLabel = filterByMonth ? parsedMonth.ToString("MMMM_yyyy") : opdList.First().Date.Year.ToString();
            string fileName = $"OPD_OPDAssisted_{monthLabel}_{mswName}";

            // Sanitize sheet name (for Excel)
            string safeSheetName = Regex.Replace(fileName, @"[\[\]\*\?/\\:]", "_");
            if (safeSheetName.Length > 31)
                safeSheetName = safeSheetName.Substring(0, 31);

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(safeSheetName);

            // HEADERS

            var headers = new[]
            {
                "MSW", "Date", "Name of Patient", "Assistance Needed", "Amount",
                "Amount Extended", "Resources"
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
            cell2.Value = "OPD Assisted";
            cell2.Style.Font.Bold = true;
            cell2.Style.Font.FontSize = 12;
            cell2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range(2, 1, 2, headers.Count()).Merge();

            // Column 3
            var cell3 = worksheet.Cell(3, 1);
            cell3.Value = $"{monthLabel} OPD";
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
            foreach (var opd in opdList)
            {
                worksheet.Cell(dataRow, 1).Value = opd.MSW;
                worksheet.Cell(dataRow, 2).Value = opd.Date.ToShortDateString();
                worksheet.Cell(dataRow, 3).Value = $"{opd.LastName}, {opd.FirstName} {opd.MiddleName}";
                worksheet.Cell(dataRow, 4).Value = opd.AssistanceNeeded;
                worksheet.Cell(dataRow, 5).Value = opd.Amount;
                worksheet.Cell(dataRow, 6).Value = opd.AmountExtended;
                worksheet.Cell(dataRow, 7).Value = opd.Resources;

                dataRow++;
            }

            ExcelReportStyler.ApplyWorksheetDesign(worksheet, new List<int> { 1, 2, 3 }, new List<int> { headerRow }, new List<int> { dataRow }, dataRow, User.FindFirst(ClaimTypes.Name).Value, false, true);

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

        public async Task<IActionResult> ExportOPDGLReceivedToExcel(int userID, string? month)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            // Parse the month input if provided
            bool filterByMonth = DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedMonth);

            var query = context.OPD.AsQueryable();

            if (userID > 0)
            {
                query = query.Where(opd => opd.UserID == userID);
            }

            if (filterByMonth)
            {
                query = query.Where(opd => opd.Date.Month == parsedMonth.Month && opd.Date.Year == parsedMonth.Year);
            }

            var opdList = await query.ToListAsync();

            if (opdList == null || !opdList.Any())
            {
                TempData["ErrorMessage"] = "No OPD records found for selected filters.";
                return RedirectToAction("OPDAssisted");
            }

            // File name generation
            string mswName = userID > 0 ? opdList.First().MSW : "All MSW";
            string monthLabel = filterByMonth ? parsedMonth.ToString("MMMM_yyyy") : opdList.First().Date.Year.ToString();
            string fileName = $"OPD_GLReceived_{monthLabel}_{mswName}";

            // Sanitize sheet name (for Excel)
            string safeSheetName = Regex.Replace(fileName, @"[\[\]\*\?/\\:]", "_");
            if (safeSheetName.Length > 31)
                safeSheetName = safeSheetName.Substring(0, 31);

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(safeSheetName);

            // HEADERS

            var headers = new[]
            {
                "MSW", "Date", "Name of Patient", "Resources", "Proponent of GL",
                "Received GL"
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
            cell2.Value = "OPD GL Received";
            cell2.Style.Font.Bold = true;
            cell2.Style.Font.FontSize = 12;
            cell2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range(2, 1, 2, headers.Count()).Merge();

            // Column 3
            var cell3 = worksheet.Cell(3, 1);
            cell3.Value = $"{monthLabel} OPD";
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
            foreach (var opd in opdList)
            {
                worksheet.Cell(dataRow, 1).Value = opd.MSW;
                worksheet.Cell(dataRow, 2).Value = opd.Date.ToShortDateString();
                worksheet.Cell(dataRow, 3).Value = $"{opd.LastName}, {opd.FirstName} {opd.MiddleName}";
                worksheet.Cell(dataRow, 4).Value = opd.Resources;
                worksheet.Cell(dataRow, 5).Value = opd.GLProponent;
                worksheet.Cell(dataRow, 6).Value = opd.GLAmountReceived;

                dataRow++;
            }

            ExcelReportStyler.ApplyWorksheetDesign(worksheet, new List<int> { 1, 2, 3 }, new List<int> { headerRow }, new List<int> { dataRow }, dataRow, User.FindFirst(ClaimTypes.Name).Value, false, true);

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

        public async Task<IActionResult> Statistics()
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var opdList = await context.OPD.ToListAsync();
            if (opdList == null || !opdList.Any())
            {
                TempData["ErrorMessage"] = "No OPD records found.";
                return RedirectToAction("Index");
            }

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            var viewModel = new OPDViewModel
            {
                OPDList = opdList,
                Users = users
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ExportOPDStatisticsToExcel(int userID, string? month)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            // Parse the month input if provided
            bool filterByMonth = DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedMonth);

            var query = context.OPD.AsQueryable();

            if (userID > 0)
            {
                query = query.Where(opd => opd.UserID == userID);
            }

            if (filterByMonth)
            {
                query = query.Where(opd => opd.Date.Month == parsedMonth.Month && opd.Date.Year == parsedMonth.Year);
            }

            var opdList = await query.ToListAsync();

            if (opdList == null || !opdList.Any())
            {
                TempData["ErrorMessage"] = "No OPD records found for selected filters.";
                return RedirectToAction("Statistics");
            }

            // File name generation
            string mswName = userID > 0 ? opdList.First().MSW : "All MSW";
            string monthLabel = filterByMonth ? parsedMonth.ToString("MMMM_yyyy") : opdList.First().Date.Year.ToString();
            string fileName = $"OPD_Statistics_{monthLabel}_{mswName}";

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
            cell2.Value = "OPD Statistics";
            cell2.Style.Font.Bold = true;
            cell2.Style.Font.FontSize = 12;
            cell2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range(2, 1, 2, headers.Count()).Merge();

            // Column 3
            var cell3 = worksheet.Cell(3, 1);
            cell3.Value = $"{monthLabel} OPD";
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
                    var count = opdList.Count(opd => opd.SourceOfReferral.Equals(value, StringComparison.OrdinalIgnoreCase) && opd.Date.Month == i);
                    worksheet.Cell(referralRow, i + 1).Value = count == 0 ? "" : count;
                    worksheet.Cell(referralRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                worksheet.Cell(referralRow, 8).Value =
                    Enumerable.Range(1, 6).Sum(i => opdList.Count(opd => opd.SourceOfReferral.Equals(value, StringComparison.OrdinalIgnoreCase) && opd.Date.Month == i));
                worksheet.Cell(referralRow, 8).Style.Font.Bold = true;
                worksheet.Cell(referralRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                for (int i = 7; i <= 12; i++)
                {
                    var count = opdList.Count(opd => opd.SourceOfReferral.Equals(value, StringComparison.OrdinalIgnoreCase) && opd.Date.Month == i);
                    worksheet.Cell(referralRow, i + 2).Value = count == 0 ? "" : count;
                    worksheet.Cell(referralRow, i + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                worksheet.Cell(referralRow, 15).Value =
                    Enumerable.Range(7, 12).Sum(i => opdList.Count(opd => opd.SourceOfReferral.Equals(value, StringComparison.OrdinalIgnoreCase) && opd.Date.Month == i));
                worksheet.Cell(referralRow, 15).Style.Font.Bold = true;
                worksheet.Cell(referralRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                referralRow++;
            }

            worksheet.Cell(referralRow, 1).Value = "TOTAL";
            worksheet.Cell(referralRow, 1).Style.Font.Bold = true;
            worksheet.Cell(referralRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int i = 1; i <= 6; i++)
            {
                var count = opdList.Count(opd => opd.Date.Month == i);
                worksheet.Cell(referralRow, i + 1).Value = count;
                worksheet.Cell(referralRow, i + 1).Style.Font.Bold = true;
                worksheet.Cell(referralRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(referralRow, 8).Value =
                Enumerable.Range(1, 6).Sum(i => opdList.Count(opd => opd.Date.Month == i));
            worksheet.Cell(referralRow, 8).Style.Font.Bold = true;
            worksheet.Cell(referralRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int i = 7; i <= 12; i++)
            {
                var count = opdList.Count(opd => opd.Date.Month == i);
                worksheet.Cell(referralRow, i + 2).Value = count;
                worksheet.Cell(referralRow, i + 2).Style.Font.Bold = true;
                worksheet.Cell(referralRow, i + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(referralRow, 15).Value =
                Enumerable.Range(7, 12).Sum(i => opdList.Count(opd => opd.Date.Month == i));
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
                var count = opdList.Count(opd => !opd.IsOld && opd.Date.Month == i);
                worksheet.Cell(caseloadRow, i + 1).Value = count == 0 ? "" : count;
                worksheet.Cell(caseloadRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(caseloadRow, 8).Value =
                Enumerable.Range(1, 6).Sum(i => opdList.Count(opd => !opd.IsOld && opd.Date.Month == i));
            worksheet.Cell(caseloadRow, 8).Style.Font.Bold = true;
            worksheet.Cell(caseloadRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int i = 7; i <= 12; i++)
            {
                var count = opdList.Count(opd => !opd.IsOld && opd.Date.Month == i);
                worksheet.Cell(caseloadRow, i + 2).Value = count == 0 ? "" : count;
                worksheet.Cell(caseloadRow, i + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(caseloadRow, 15).Value =
                Enumerable.Range(7, 12).Sum(i => opdList.Count(opd => !opd.IsOld && opd.Date.Month == i));
            worksheet.Cell(caseloadRow, 15).Style.Font.Bold = true;
            worksheet.Cell(caseloadRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            caseloadRow++;

            worksheet.Cell(caseloadRow, 1).Value = "1.2 Old Cases";
            worksheet.Cell(caseloadRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(caseloadRow, 1).Style.Alignment.Indent = 2;

            for (int i = 1; i <= 6; i++)
            {
                var count = opdList.Count(opd => opd.IsOld && opd.Date.Month == i);
                worksheet.Cell(caseloadRow, i + 1).Value = count == 0 ? "" : count;
                worksheet.Cell(caseloadRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(caseloadRow, 8).Value =
                Enumerable.Range(1, 6).Sum(i => opdList.Count(opd => opd.IsOld && opd.Date.Month == i));
            worksheet.Cell(caseloadRow, 8).Style.Font.Bold = true;
            worksheet.Cell(caseloadRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int i = 7; i <= 12; i++)
            {
                var count = opdList.Count(opd => opd.IsOld && opd.Date.Month == i);
                worksheet.Cell(caseloadRow, i + 2).Value = count == 0 ? "" : count;
                worksheet.Cell(caseloadRow, i + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(caseloadRow, 15).Value =
                Enumerable.Range(7, 12).Sum(i => opdList.Count(opd => opd.IsOld && opd.Date.Month == i));
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
                var count = opdList.Count(opd => opd.IsPWD && opd.Date.Month == i);
                worksheet.Cell(caseloadRow, i + 1).Value = count == 0 ? "" : count;
                worksheet.Cell(caseloadRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(caseloadRow, 8).Value =
                Enumerable.Range(1, 6).Sum(i => opdList.Count(opd => opd.IsPWD && opd.Date.Month == i));
            worksheet.Cell(caseloadRow, 8).Style.Font.Bold = true;
            worksheet.Cell(caseloadRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int i = 7; i <= 12; i++)
            {
                var count = opdList.Count(opd => opd.IsPWD && opd.Date.Month == i);
                worksheet.Cell(caseloadRow, i + 2).Value = count == 0 ? "" : count;
                worksheet.Cell(caseloadRow, i + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(caseloadRow, 15).Value =
                Enumerable.Range(7, 12).Sum(i => opdList.Count(opd => opd.IsPWD && opd.Date.Month == i));
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

            var documentations = new List<string>
            {
                "1.29.1 Profile",
                "1.29.2 Progress Notes",
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
                var count = opdList.Count(opd => opd.Date.Month == i);
                worksheet.Cell(serviceRow, i + 1).Value = count;
                worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(serviceRow, 8).Value =
                Enumerable.Range(1, 6).Sum(i => opdList.Count(opd => opd.Date.Month == i));
            worksheet.Cell(serviceRow, 8).Style.Font.Bold = true;
            worksheet.Cell(serviceRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int i = 7; i <= 12; i++)
            {
                var count = opdList.Count(opd => opd.Date.Month == i);
                worksheet.Cell(serviceRow, i + 2).Value = count;
                worksheet.Cell(serviceRow, i + 2).Style.Font.Bold = true;
                worksheet.Cell(serviceRow, i + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(serviceRow, 15).Value =
                Enumerable.Range(7, 12).Sum(i => opdList.Count(opd => opd.Date.Month == i));
            worksheet.Cell(serviceRow, 15).Style.Font.Bold = true;
            worksheet.Cell(serviceRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ExcelReportStyler.ApplyWorksheetDesign(worksheet, new List<int> { 1, 2, 3 }, new List<int> { headerRow }, new List<int> { referralRow, serviceRow }, serviceRow, User.FindFirst(ClaimTypes.Name).Value, false, false, true);

            // Autofit for better presentation
            worksheet.Column(1).AdjustToContents();

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
