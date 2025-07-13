using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using LittleArkFoundation.Areas.Admin.Data;
using LittleArkFoundation.Areas.Admin.Models.Discharges;
using LittleArkFoundation.Areas.Admin.Models.Form;
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
    [HasPermission("ManageOPD")]
    public class OPDController : Controller
    {
        private readonly ConnectionService _connectionService;
        public OPDController(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<IActionResult> Index(string? sortToggle)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            string sortToggleValue = sortToggle ?? "All";
            ViewBag.sortToggle = sortToggleValue;

            var opdList = new List<OPDModel>();
            if (sortToggleValue == "All")
            {
                // Fetch all OPD records
                opdList = await context.OPD.ToListAsync();
            }
            else if (sortToggleValue == "Admitted")
            {
                // Fetch only admitted patients
                opdList = await context.OPD.Where(opd => opd.IsAdmitted).ToListAsync();
            }
            else if (sortToggleValue == "Not Admitted")
            {
                // Fetch only non-admitted patients
                opdList = await context.OPD.Where(opd => !opd.IsAdmitted).ToListAsync();
            }

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
                Users = users
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

            foreach (var word in searchWords)
            {
                var term = word.Trim();

                query = query.Where(u =>
                    EF.Functions.Like(u.FirstName, $"%{term}%") ||
                    EF.Functions.Like(u.MiddleName, $"%{term}%") ||
                    EF.Functions.Like(u.LastName, $"%{term}%") ||
                    EF.Functions.Like(u.Id.ToString(), $"%{term}%"));
            }

            var opdList = await query.ToListAsync();

            var scoredList = new List<(OPDModel opd, Dictionary<string, int> scores, bool isEligible)>();
            var _scoreService = new OPDScoringService(connectionString);
            foreach (var opd in opdList)
            {
                var scores = await _scoreService.GetWeightedScoresAsync(opd);
                var isEligible = await _scoreService.IsEligibleForAdmissionAsync(scores.Values.Sum());
                scoredList.Add((opd, scores, isEligible));
            }

            var viewModel = new OPDViewModel
            {
                OPDList = opdList,
                OPDScoringList = scoredList,
            };

            return View("Index", viewModel);
        }

        public async Task<IActionResult> SortBy(string sortByUserID, string? sortByMonth, string? sortToggle)
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

            var opdList = await query.ToListAsync();

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
                Users = users
            };
            
            return View("Index", viewModel);
        }

        public async Task<IActionResult> SortByOPDAssistedAndReports(string sortByUserID, string? sortByMonth, string? viewName)
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
            worksheet.Cell(1, 1).Value = "COUNTA OF DATE PROCESSED BY MSW";
            worksheet.Cell(2, 1).Value = "Date Processed by MSW";

            int dateCol = 2;
            foreach (var user in users)
            {
                worksheet.Cell(2, dateCol).Value = user.Username;
                dateCol++;
            }

            worksheet.Cell(2, dateCol).Value = "Grand Total";

            // Prepare data grouped by ProcessedDate
            var groupedOPD = opdList
                .GroupBy(d => d.Date)
                .OrderBy(g => g.Key)
                .ToList();

            int dateRow = 3;
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

        public async Task<IActionResult> OPDAssisted()
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
    }
}
