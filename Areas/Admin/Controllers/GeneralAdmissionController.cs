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

        public async Task<IActionResult> SortByReports(string sortByUserID, string? sortByMonth, string? viewName = "Index")
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
                LoggingService.LogInformation($"General Admission Patient edit successful. Edited Id: {viewModel.GeneralAdmission.Id}. Edited by UserID: {User.FindFirst(ClaimTypes.NameIdentifier).Value}, DateTime: {DateTime.Now}");
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
                LoggingService.LogInformation($"General Admission Patient creation successful. Created Id: {viewModel.GeneralAdmission.Id}. Created by UserID: {userIdClaim}, DateTime: {DateTime.Now}");
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

        public async Task<IActionResult> ExportReportsToExcel(int userID, string? month)
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

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            // HEADERS
            // COUNTA OF DATE PROCESSED BY MSW
            worksheet.Cell(1, 1).Value = "COUNTA OF DATE PROCESSED BY MSW";
            worksheet.Cell(2, 1).Value = "Date Processed by MSW";

            int dateColIndex = 2;
            foreach (var user in users)
            {
                worksheet.Cell(2, dateColIndex).Value = user.Username;
                dateColIndex++;
            }

            worksheet.Cell(2, dateColIndex).Value = "Grand Total";

            // Prepare data grouped by ProcessedDate
            var groupedOPD = generalAdmissions
                .GroupBy(d => d.Date)
                .OrderBy(g => g.Key)
                .ToList();

            int dateRowIndex = 3;
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

            // HEADERS
            // COUNTA OF CLASS 
            int classRowIndex = totalDateRowIndex + 2;

            worksheet.Cell(classRowIndex, 1).Value = "COUNTA OF CLASS";
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
            // COUNTA OF GENDER
            int genderRowIndex = totalClassRowIndex + 2;
            worksheet.Cell(genderRowIndex, 1).Value = "COUNTA OF GENDER";
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
            // COUNTA OF Eco. Stat
            int economicStatusRowIndex = totalGenderRowIndex + 2;
            worksheet.Cell(economicStatusRowIndex, 1).Value = "COUNTA OF Eco. Stat";
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
            // COUNTA OF Marital Status
            int maritalStatusRowIndex = totalEconomicStatusRowIndex + 2;
            worksheet.Cell(maritalStatusRowIndex, 1).Value = "COUNTA OF Marital Status";
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
            // COUNTA OF REFERRAL
            int referralRowIndex = totalMaritalStatusRowIndex + 2;
            worksheet.Cell(referralRowIndex, 1).Value = "COUNTA OF REFERRAL";
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
            // COUNTA OF ORIGIN
            int originRowIndex = totalReferralRowIndex + 2;
            worksheet.Cell(originRowIndex, 1).Value = "COUNTA OF ORIGIN";
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
            // COUNTA OF AGE
            int ageRowIndex = totalOriginRowIndex + 2;
            worksheet.Cell(ageRowIndex, 1).Value = "COUNTA OF AGE";
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
            // COUNTA OF PATIENT EDU. ATTAINMENT
            int patientEduRowIndex = totalAgeRowIndex + 2;
            worksheet.Cell(patientEduRowIndex, 1).Value = "COUNTA OF PATIENT EDU. ATTAINMENT";
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
            // COUNTA OF FATHER EDU. ATTAINMENT
            int fatherEduRowIndex = totalPatientEduRowIndex + 2;
            worksheet.Cell(fatherEduRowIndex, 1).Value = "COUNTA OF FATHER EDU. ATTAINMENT";
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
            // COUNTA OF MOTHER EDU. ATTAINMENT
            int motherEduRowIndex = totalFatherEduRowIndex + 2;
            worksheet.Cell(motherEduRowIndex, 1).Value = "COUNTA OF MOTHER EDU. ATTAINMENT";
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
            // COUNTA OF OLD/NEW
            int oldNewRowIndex = totalMotherEduRowIndex + 2;
            worksheet.Cell(oldNewRowIndex, 1).Value = "COUNTA OF OLD/NEW";
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
            // COUNTA OF PWD
            int pwdRowIndex = oldNewDataRowIndex + 2;
            worksheet.Cell(pwdRowIndex, 1).Value = "COUNTA OF PWD";
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
            // COUNTA OF HOUSEHOLD SIZE
            int householdSizeRowIndex = pwdDataRowIndex + 2;
            worksheet.Cell(householdSizeRowIndex, 1).Value = "COUNTA OF HOUSEHOLD SIZE";
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
            // COUNTA OF LIGHT SOURCE
            int lightSourceRowIndex = totalHouseholdSizeRowIndex + 2;
            worksheet.Cell(lightSourceRowIndex, 1).Value = "COUNTA OF LIGHT SOURCE";
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
            // COUNTA OF WATER SOURCE
            int waterSourceRowIndex = totalLightSourceRowIndex + 2;
            worksheet.Cell(waterSourceRowIndex, 1).Value = "COUNTA OF WATER SOURCE";
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
            // COUNTA OF FUEL SOURCE
            int fuelSourceRowIndex = totalWaterSourceRowIndex + 2;
            worksheet.Cell(fuelSourceRowIndex, 1).Value = "COUNTA OF FUEL SOURCE";
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
            // COUNTA OF DWELLING TYPE
            int dwellingTypeRowIndex = totalFuelSourceRowIndex + 2;
            worksheet.Cell(dwellingTypeRowIndex, 1).Value = "COUNTA OF DWELLING TYPE";
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
            // COUNTA OF STATS OCCUPATION
            int statsOccupationRowIndex = totalDwellingTypeRowIndex + 2;
            worksheet.Cell(statsOccupationRowIndex, 1).Value = "COUNTA OF STATS OCCUPATION";
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
            // COUNTA OF INCOME RANGE
            int incomeRangeRowIndex = totalStatsOccupationRowIndex + 2;
            worksheet.Cell(incomeRangeRowIndex, 1).Value = "COUNTA OF INCOME RANGE";
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
            // COUNTA OF OCCUPATION
            int occupationRowIndex = totalIncomeRangeRowIndex + 2;
            worksheet.Cell(occupationRowIndex, 1).Value = "COUNTA OF OCCUPATION";
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
            // COUNTA OF MONTHLY INCOME
            int monthlyIncomeRowIndex = totalOccupationRowIndex + 2;
            worksheet.Cell(monthlyIncomeRowIndex, 1).Value = "COUNTA OF MONTHLY INCOME";
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
