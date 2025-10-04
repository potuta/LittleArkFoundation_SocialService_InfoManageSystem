using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using DocumentFormat.OpenXml.Spreadsheet;
using LittleArkFoundation.Areas.Admin.Data;
using LittleArkFoundation.Areas.Admin.Models.Discharges;
using LittleArkFoundation.Areas.Admin.Models.Form;
using LittleArkFoundation.Areas.Admin.Models.OPD;
using LittleArkFoundation.Areas.Admin.Models.Statistics;
using LittleArkFoundation.Areas.Admin.Services.Reports;
using LittleArkFoundation.Areas.Admin.Services.Statistics;
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

            var latestIds = await context.OPD
                .GroupBy(o => o.OPDId)
                .Select(g => g.Max(o => o.Id)) // get latest Id per OPDId
                .ToListAsync();

            var latestRecords = await context.OPD
                .Where(o => latestIds.Contains(o.Id))
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            if (sortToggleValue == "Admitted")
            {
                latestRecords = latestRecords.Where(o => o.IsAdmitted).ToList();
            }
            else if (sortToggleValue == "Not Admitted")
            {
                latestRecords = latestRecords.Where(o => !o.IsAdmitted).ToList();
            }

            if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime month))
            {
                latestRecords = latestRecords
                    .Where(o => o.Date.Month == month.Month && o.Date.Year == month.Year)
                    .ToList();
                ViewBag.sortByMonth = month.ToString("yyyy-MM");
            }

            var totalCount = latestRecords.Count;
            var opdList = latestRecords
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var scoredList = new List<(OPDModel opd, Dictionary<string, int> scores, bool isEligible)>();
            var _scoreService = new OPDScoringService(connectionString);
            foreach (var opd in opdList)
            {
                var scores = await _scoreService.GetWeightedScoresAsync(opd);
                var isEligible = await _scoreService.IsEligibleForAdmissionAsync(scores.Values.Sum());
                scoredList.Add((opd, scores, isEligible));
            }

            var users = await context.Users.ToListAsync();

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

        public async Task<IActionResult> ViewHistory(int opdId)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var opdList = await context.OPD
                .Where(o => o.OPDId == opdId)
                .OrderByDescending(o => o.Id)
                .ThenByDescending(o => o.Date)
                .ToListAsync();

            if (opdList == null || opdList.Count == 0)
            {
                TempData["ErrorMessage"] = "No history found for the specified OPD ID.";
                return RedirectToAction("Index");
            }

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

            return View(viewModel);
        }

        [HasPermission("CreateOPD")]
        public async Task<IActionResult> ReAssessment(int opdId)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);
            
            var existingOPD = await context.OPD
                .Where(o => o.OPDId == opdId)
                .OrderByDescending(o => o.Id)
                .ThenByDescending(o => o.Date)
                .FirstOrDefaultAsync();

            var existingOPDPatient = await context.OPDPatients.FirstOrDefaultAsync(p => p.OPDId == opdId);

            if (existingOPD == null)
            {
                TempData["ErrorMessage"] = "No history found for the specified OPD ID.";
                return RedirectToAction("Index");
            }

            var viewModel = new OPDViewModel
            {
                OPDId = existingOPD.OPDId,
                OPD = new OPDModel
                {
                    OPDId = existingOPD.OPDId,
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    IsOld = true,
                    IsAdmitted = false,
                    Class = existingOPD.Class,
                    FirstName = existingOPD.FirstName,
                    LastName = existingOPD.LastName,
                    MiddleName = existingOPD.MiddleName,
                    ContactNo = existingOPD.ContactNo,
                    Age = existingOPD.Age,
                    Gender = existingOPD.Gender,
                    IsPWD = existingOPD.IsPWD,
                    Diagnosis = existingOPD.Diagnosis,
                    Address = existingOPD.Address,
                    SourceOfReferral = existingOPD.SourceOfReferral,
                    MotherFirstName = existingOPD.MotherFirstName,
                    MotherMiddleName = existingOPD.MotherMiddleName,
                    MotherLastName = existingOPD.MotherLastName,
                    MotherOccupation = existingOPD.MotherOccupation,
                    FatherFirstName = existingOPD.FatherFirstName,
                    FatherMiddleName = existingOPD.FatherMiddleName,
                    FatherLastName = existingOPD.FatherLastName,
                    FatherOccupation = existingOPD.FatherOccupation,
                    MonthlyIncome = existingOPD.MonthlyIncome,
                    NoOfChildren = existingOPD.NoOfChildren,
                    AssistanceNeeded = existingOPD.AssistanceNeeded,
                    Amount = existingOPD.Amount,
                    PtShare = existingOPD.PtShare,
                    AmountExtended = existingOPD.AmountExtended,
                    Resources = existingOPD.Resources,
                    GLProponent = existingOPD.GLProponent,
                    GLAmountReceived = existingOPD.GLAmountReceived,
                    MSW = User?.FindFirstValue(ClaimTypes.Name) ?? "N/A",
                    Category = existingOPD.Category,
                    UserID = int.Parse(User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0")
                },
                OPDPatient = new OPDPatientsModel
                {
                    OPDId = existingOPDPatient.OPDId,
                    FirstName = existingOPDPatient.FirstName,
                    LastName = existingOPDPatient.LastName,
                    MiddleName = existingOPDPatient.MiddleName,
                }
            };

            return View("Create", viewModel);

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

            var latestIds = await context.OPD
                .GroupBy(o => o.OPDId)
                .Select(g => g.Max(o => o.Id)) // get latest Id per OPDId
                .ToListAsync();

            var latestRecords = await context.OPD
                .Where(o => latestIds.Contains(o.Id))
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            if (sortToggleValue == "Admitted")
            {
                latestRecords = latestRecords.Where(o => o.IsAdmitted).ToList();
            }
            else if (sortToggleValue == "Not Admitted")
            {
                latestRecords = latestRecords.Where(o => !o.IsAdmitted).ToList();
            }

            if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime month))
            {
                latestRecords = latestRecords
                    .Where(o => o.Date.Month == month.Month && o.Date.Year == month.Year)
                    .ToList();
                ViewBag.sortByMonth = month.ToString("yyyy-MM");
            }

            foreach (var word in searchWords)
            {
                var term = word.Trim().ToLower();

                latestRecords = latestRecords.Where(u =>
                    (!string.IsNullOrEmpty(u.FirstName) && u.FirstName.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(u.MiddleName) && u.MiddleName.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(u.LastName) && u.LastName.ToLower().Contains(term)) ||
                    u.OPDId.ToString().Contains(term)
                ).ToList();
            }

            // Pagination
            var totalCount = latestRecords.Count;
            var opdList = latestRecords
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            var scoredList = new List<(OPDModel opd, Dictionary<string, int> scores, bool isEligible)>();
            var _scoreService = new OPDScoringService(connectionString);
            foreach (var opd in opdList)
            {
                var scores = await _scoreService.GetWeightedScoresAsync(opd);
                var isEligible = await _scoreService.IsEligibleForAdmissionAsync(scores.Values.Sum());
                scoredList.Add((opd, scores, isEligible));
            }

            //var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            //var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();
            var users = await context.Users.ToListAsync();

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

            var latestIds = await context.OPD
                .GroupBy(o => o.OPDId)
                .Select(g => g.Max(o => o.Id)) // get latest Id per OPDId
                .ToListAsync();

            var latestRecords = await context.OPD
                .Where(o => latestIds.Contains(o.Id))
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            if (!string.IsNullOrEmpty(sortByUserID))
            {
                latestRecords = latestRecords.Where(opd => opd.UserID == int.Parse(sortByUserID)).ToList();
                var user = await context.Users.FindAsync(int.Parse(sortByUserID));
                ViewBag.sortBy = user.Username;
                ViewBag.sortByUserID = user.UserID.ToString();
            }

            if (sortToggleValue == "Admitted")
            {
                latestRecords = latestRecords.Where(o => o.IsAdmitted).ToList();
            }
            else if (sortToggleValue == "Not Admitted")
            {
                latestRecords = latestRecords.Where(o => !o.IsAdmitted).ToList();
            }

            if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime month))
            {
                latestRecords = latestRecords
                    .Where(o => o.Date.Month == month.Month && o.Date.Year == month.Year)
                    .ToList();
                ViewBag.sortByMonth = month.ToString("yyyy-MM");
            }

            var totalCount = latestRecords.Count;
            var opdList = latestRecords
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var scoredList = new List<(OPDModel opd, Dictionary<string, int> scores, bool isEligible)>();
            var _scoreService = new OPDScoringService(connectionString);
            foreach (var opd in opdList)
            {
                var scores = await _scoreService.GetWeightedScoresAsync(opd);
                var isEligible = await _scoreService.IsEligibleForAdmissionAsync(scores.Values.Sum());
                scoredList.Add((opd, scores, isEligible));
            }

            //var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            //var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();
            var users = await context.Users.ToListAsync();

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

        public async Task<IActionResult> SortByReports(string sortByUserID, string? sortToggle, string? sortByMonth, int page = 1, int pageSize = 20)
        {
            string sortToggleValue = sortToggle ?? "OPD";
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

            if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime month))
            {
                query = query.Where(opd => opd.Date.Month == month.Month && opd.Date.Year == month.Year);
                ViewBag.sortByMonth = month.ToString("yyyy-MM");
            }

            List<OPDModel> opdList = new List<OPDModel>();
            int totalCount = 0;

            if (sortToggleValue == "OPDAssisted" || sortToggleValue == "GLReceived")
            {
                // Pagination
                totalCount = await query.CountAsync();
                opdList = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            else
            {
                opdList = await query.ToListAsync();
            }

            //var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            //var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();
            var users = await context.Users.ToListAsync();

            var viewModel = new OPDViewModel
            {
                OPDList = opdList,
                Users = users,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
            };

            return View("Reports", viewModel);
        }

        public async Task<IActionResult> SortByStatistics(string sortByUserID, string? sortByMonth, string? viewName)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            IQueryable<OPDModel> query = context.OPD.AsQueryable();
            var statisticsQuery = context.Statistics.AsQueryable();

            if (!string.IsNullOrEmpty(sortByUserID))
            {
                query = query.Where(opd => opd.UserID == int.Parse(sortByUserID));
                statisticsQuery = statisticsQuery.Where(stat => stat.UserID == int.Parse(sortByUserID));
                var user = await context.Users.FindAsync(int.Parse(sortByUserID));
                ViewBag.sortBy = user.Username;
                ViewBag.sortByUserID = user.UserID.ToString();
            }

            if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime month))
            {
                query = query.Where(opd => opd.Date.Month == month.Month && opd.Date.Year == month.Year);
                statisticsQuery = statisticsQuery.Where(stat => stat.Date.Value.Month == month.Month && stat.Date.Value.Year == month.Year);
                ViewBag.sortByMonth = month.ToString("yyyy-MM");
            }

            var opdList = await query.ToListAsync();
            var statisticsList = await statisticsQuery.Where(s => s.Type == "OPD").ToListAsync();

            //var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            //var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();
            var users = await context.Users.ToListAsync();

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

            var totalSourcesMonthly = new Dictionary<int, int>();
            for (int i = 1; i <= 12; i++)
            {
                totalSourcesMonthly[i] = sourceOfReferral.Sum(source =>
                    opdList.Count(m =>
                        string.Equals(m.SourceOfReferral, source.Value, StringComparison.OrdinalIgnoreCase) &&
                        m.Date.Month == i));
            }

            var totalCaseloadMonthly = new Dictionary<int, int>();
            for (int i = 1; i <= 12; i++)
            {
                totalCaseloadMonthly[i] =
                    opdList.Count(m => !m.IsOld && m.Date.Month == i) +
                    opdList.Count(m => m.IsOld && m.Date.Month == i) +
                    opdList.Count(m => m.IsPWD && m.Date.Month == i);
            }

            var totalOPDMonthlyDictionary = new Dictionary<int, int>();
            var totalStatisticsMonthlyDictionary = new Dictionary<int, Dictionary<string, int>>();
            for (int i = 1; i <= 12; i++)
            {
                totalOPDMonthlyDictionary[i] = opdList.Count(o => o.Date.Month == i);
                totalStatisticsMonthlyDictionary[i] = StatisticsHelper.SumForMonth(statisticsList, i);
            }

            var viewModel = new OPDViewModel
            {
                OPDList = opdList,
                StatisticsList = statisticsList,
                Users = users,
                TotalSourcesMonthly = totalSourcesMonthly,
                TotalCaseloadMonthly = totalCaseloadMonthly,
                TotalOPDMonthly = totalOPDMonthlyDictionary,
                TotalStatisticsMonthly = totalStatisticsMonthlyDictionary
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

                if (viewModel.OPDId > 0)
                {
                    viewModel.OPD.OPDId = viewModel.OPDId; 
                    viewModel.OPD.IsOld = true; 
                    viewModel.OPD.MSW = User?.FindFirstValue(ClaimTypes.Name) ?? "N/A";
                    viewModel.OPD.UserID = int.Parse(User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                }
                else
                {
                    // Assign a new OPDId
                    await context.OPDPatients.AddAsync(viewModel.OPDPatient);
                    await context.SaveChangesAsync();
                    viewModel.OPD.OPDId = viewModel.OPDPatient.OPDId;
                    viewModel.OPD.IsOld = false;
                }

                await context.OPD.AddAsync(viewModel.OPD);
                await context.SaveChangesAsync();

                var existingOPDList = await context.OPD
                            .Where(o => o.OPDId == viewModel.OPD.OPDId)
                            .ToListAsync();                

                if (existingOPDList != null)
                {
                    int count = 0;
                    foreach (var existingOPD in existingOPDList)
                    {
                        if (existingOPD.FirstName != viewModel.OPD.FirstName ||
                            existingOPD.LastName != viewModel.OPD.LastName ||
                            existingOPD.MiddleName != viewModel.OPD.MiddleName)
                        {
                            while (count == 0)
                            {

                                var existingPatient = await context.OPDPatients
                                    .FirstOrDefaultAsync(p => p.OPDId == existingOPD.OPDId);

                                if (existingPatient != null)
                                {
                                    existingPatient.FirstName = viewModel.OPD.FirstName;
                                    existingPatient.LastName = viewModel.OPD.LastName;
                                    existingPatient.MiddleName = viewModel.OPD.MiddleName;
                                }

                                count++;
                            }

                            existingOPD.FirstName = viewModel.OPD.FirstName;
                            existingOPD.LastName = viewModel.OPD.LastName;
                            existingOPD.MiddleName = viewModel.OPD.MiddleName;

                        }
                    }
                    await context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Successfully created new OPD";
                LoggingService.LogInformation($"UserID: {User.FindFirst(ClaimTypes.NameIdentifier).Value}. OPD Patient creation successful. Created OPD Id: {viewModel.OPD.Id}");
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

                var existingOPDList = await context.OPD
                            .Where(o => o.OPDId == viewModel.OPD.OPDId)
                            .ToListAsync();                

                if (existingOPDList != null)
                {
                    int count = 0;
                    foreach (var existingOPD in existingOPDList)
                    {
                        if (existingOPD.FirstName != viewModel.OPD.FirstName ||
                            existingOPD.LastName != viewModel.OPD.LastName ||
                            existingOPD.MiddleName != viewModel.OPD.MiddleName)
                        {
                            while (count == 0)
                            {

                                var existingPatient = await context.OPDPatients
                                    .FirstOrDefaultAsync(p => p.OPDId == existingOPD.OPDId);

                                if (existingPatient != null)
                                {
                                    existingPatient.FirstName = viewModel.OPD.FirstName;
                                    existingPatient.LastName = viewModel.OPD.LastName;
                                    existingPatient.MiddleName = viewModel.OPD.MiddleName;
                                }

                                count++;
                            }

                            existingOPD.FirstName = viewModel.OPD.FirstName;
                            existingOPD.LastName = viewModel.OPD.LastName;
                            existingOPD.MiddleName = viewModel.OPD.MiddleName;

                        }
                    }
                    await context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"Successfully edited/updated OPD Id: {viewModel.OPD.Id}";
                LoggingService.LogInformation($"UserID: {User.FindFirst(ClaimTypes.NameIdentifier).Value}. OPD Patient edited/updated successful. Updated OPD Id: {viewModel.OPD.Id}");
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
                LoggingService.LogInformation($"UserID: {User.FindFirst(ClaimTypes.NameIdentifier).Value}. OPD Patient deleted successful. Deleted OPD Id: {id}");
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

            await using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                stream.Position = 0;
                return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"{fileName}.xlsx");
            }
        }

        public async Task<IActionResult> Reports(string? sortToggle, string? sortByMonth, int page = 1, int pageSize = 20)
        {
            string sortToggleValue = sortToggle ?? "OPD";
            ViewBag.sortToggle = sortToggleValue;

            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var query = context.OPD.AsQueryable();

            if (query == null || !query.Any())
            {
                TempData["ErrorMessage"] = "No OPD records found.";
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime month))
            {
                query = query.Where(patient => patient.Date.Month == month.Month && patient.Date.Year == month.Year);
                ViewBag.sortByMonth = month.ToString("yyyy-MM");
            }

            List<OPDModel> opdList = new List<OPDModel>();
            int totalCount = 0;

            if (sortToggleValue == "OPDAssisted" || sortToggleValue == "GLReceived")
            {
                // Pagination
                totalCount = await query.CountAsync();
                opdList = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            else
            {
                opdList = await query.ToListAsync();
            }

            //var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            //var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();
            var users = await context.Users.ToListAsync();

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

        public async Task<IActionResult> ExportReportsToExcel(int userID, string? month, string? sortToggle)
        {
            if (sortToggle == "OPD")
            {
                return RedirectToAction("ExportOPDReportsToExcel", new { userID, month });
            }
            else if (sortToggle == "OPDAssisted")
            {
                return RedirectToAction("ExportOPDAssistedToExcel", new { userID, month });
            }
            else if (sortToggle == "GLReceived")
            {
                return RedirectToAction("ExportOPDGLReceivedToExcel", new { userID, month });
            }

            TempData["ErrorMessage"] = $"Download failed. No reports was found for {sortToggle}";
            return RedirectToAction("Reports");
        }

        public async Task<IActionResult> ExportOPDReportsToExcel(int userID, string? month)
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

            //var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            //var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();
            var users = await context.Users.ToListAsync();

            // HEADERS
            // COUNT OF DATE PROCESSED BY MSW
            worksheet.Cell(4, 1).Value = "COUNT OF DATE PROCESSED BY MSW";
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
            // COUNT OF CLASS
            int classRowStart = totalDateRow + 2;

            worksheet.Cell(classRowStart, 1).Value = "COUNT OF CLASS";
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
            // COUNT OF CLASS BY GENDER
            int genderRowStart = totalClassRow + 2;

            worksheet.Cell(genderRowStart, 1).Value = "COUNT OF CLASS BY GENDER";
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
            // COUNT OF OLD/NEW
            int oldNewRowStart = totalGenderRow + 2;

            worksheet.Cell(oldNewRowStart, 1).Value = "COUNT OF OLD/NEW";
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
            // COUNT OF PWD
            int pwdRowStart = totalOldNewRow + 2;

            worksheet.Cell(pwdRowStart, 1).Value = "COUNT OF PWD";
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
            // COUNT OF REFERRAL PROCESSED BY MSW
            int referralRowStart = totalPwdRow + 2;

            worksheet.Cell(referralRowStart, 1).Value = "COUNT OF REFERRAL BY MSW";
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

            await using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                stream.Position = 0;
                return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"{fileName}.xlsx");
            }
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
                return RedirectToAction("Reports");
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

            await using (var stream = new MemoryStream())
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
                return RedirectToAction("Reports");
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

            var opdList = await context.OPD.ToListAsync();
            var statisticsList = await context.Statistics
                .Where(s => s.Type == "OPD")
                .ToListAsync();

            var totalOPDMonthlyDictionary = new Dictionary<int, int>();
            var totalStatisticsMonthlyDictionary = new Dictionary<int, Dictionary<string, int>>();

            for (int month = 1; month <= 12; month++)
            {
                totalOPDMonthlyDictionary[month] = opdList?.Count(o => o.Date.Month == month) ?? 0;
                totalStatisticsMonthlyDictionary[month] = StatisticsHelper.SumForMonth(statisticsList, month);
            }

            if ((opdList == null || !opdList.Any()) &&
                (totalStatisticsMonthlyDictionary.Values.Sum(dict => dict.Values.Sum()) == 0))
            {
                TempData["ErrorMessage"] = "No OPD records found for selected filters.";
                return RedirectToAction("Index");
            }

            //var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            //var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();
            var users = await context.Users.ToListAsync();

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

            var totalSourcesMonthly = new Dictionary<int, int>();
            for (int month = 1; month <= 12; month++)
            {
                totalSourcesMonthly[month] = sourceOfReferral.Sum(source =>
                    opdList.Count(m =>
                        string.Equals(m.SourceOfReferral, source.Value, StringComparison.OrdinalIgnoreCase) &&
                        m.Date.Month == month));
            }

            var totalCaseloadMonthly = new Dictionary<int, int>();
            for (int month = 1; month <= 12; month++)
            {
                totalCaseloadMonthly[month] = 
                    opdList.Count(m => !m.IsOld && m.Date.Month == month) +
                    opdList.Count(m => m.IsOld && m.Date.Month == month) +
                    opdList.Count(m => m.IsPWD && m.Date.Month == month);
            }

            var viewModel = new OPDViewModel
            {
                OPDList = opdList,
                Users = users,
                StatisticsList = statisticsList,
                TotalSourcesMonthly = totalSourcesMonthly,
                TotalCaseloadMonthly = totalCaseloadMonthly,
                TotalOPDMonthly = totalOPDMonthlyDictionary,
                TotalStatisticsMonthly = totalStatisticsMonthlyDictionary
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
            var statisticsQuery = context.Statistics.AsQueryable();

            if (userID > 0)
            {
                query = query.Where(opd => opd.UserID == userID);
                statisticsQuery = statisticsQuery.Where(stat => stat.UserID == userID);
            }

            if (filterByMonth)
            {
                query = query.Where(opd => opd.Date.Month == parsedMonth.Month && opd.Date.Year == parsedMonth.Year);
                statisticsQuery = statisticsQuery.Where(stat => stat.Date.Value.Month == parsedMonth.Month && stat.Date.Value.Year == parsedMonth.Year);
            }

            var opdList = await query.ToListAsync();
            var statisticsList = await statisticsQuery
                .Where(s => s.Type == "OPD")
                .ToListAsync();

            var totalOPDMonthlyDictionary = new Dictionary<int, int>();
            var totalStatisticsMonthlyDictionary = new Dictionary<int, Dictionary<string, int>>();

            for (int i = 1; i <= 12; i++)
            {
                totalOPDMonthlyDictionary[i] = opdList?.Count(o => o.Date.Month == i) ?? 0;
                totalStatisticsMonthlyDictionary[i] = StatisticsHelper.SumForMonth(statisticsList, i);
            }

            if ((opdList == null || !opdList.Any()) &&
                (totalStatisticsMonthlyDictionary.Values.Sum(dict => dict.Values.Sum()) == 0))
            {
                TempData["ErrorMessage"] = "No OPD records found for selected filters.";
                return RedirectToAction("Statistics");
            }

            // File name generation
            var user = await context.Users.FindAsync(userID);
            string mswName = userID > 0 ? user.Username : "All MSW";

            string yearLabel;

            // Use FirstOrDefault instead of Any() + First()
            var firstOpd = opdList?.FirstOrDefault();
            if (firstOpd != null)
            {
                yearLabel = firstOpd.Date.Year.ToString();
            }
            else
            {
                var firstStat = statisticsList?.FirstOrDefault();
                yearLabel = firstStat?.Date?.Year.ToString() ?? DateTime.Now.Year.ToString();
            }

            string monthLabel = filterByMonth
                ? parsedMonth.ToString("MMMM_yyyy")
                : yearLabel;

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
                    Enumerable.Range(7, 6).Sum(i => opdList.Count(opd => opd.SourceOfReferral.Equals(value, StringComparison.OrdinalIgnoreCase) && opd.Date.Month == i));
                worksheet.Cell(referralRow, 15).Style.Font.Bold = true;
                worksheet.Cell(referralRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                referralRow++;
            }

            var totalSourcesMonthly = new Dictionary<int, int>();

            for (int i = 1; i <= 12; i++)
            {
                totalSourcesMonthly[i] = sourceOfReferral.Sum(source =>
                    opdList.Count(m =>
                        string.Equals(m.SourceOfReferral, source.Value, StringComparison.OrdinalIgnoreCase) &&
                        m.Date.Month == i));
            }

            worksheet.Cell(referralRow, 1).Value = "TOTAL";
            worksheet.Cell(referralRow, 1).Style.Font.Bold = true;
            worksheet.Cell(referralRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int i = 1; i <= 6; i++)
            {
                worksheet.Cell(referralRow, i + 1).Value = totalSourcesMonthly[i];
                worksheet.Cell(referralRow, i + 1).Style.Font.Bold = true;
                worksheet.Cell(referralRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(referralRow, 8).Value =
                Enumerable.Range(1, 6).Sum(i => totalSourcesMonthly[i]);
            worksheet.Cell(referralRow, 8).Style.Font.Bold = true;
            worksheet.Cell(referralRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int i = 7; i <= 12; i++)
            {
                worksheet.Cell(referralRow, i + 2).Value = totalSourcesMonthly[i];
                worksheet.Cell(referralRow, i + 2).Style.Font.Bold = true;
                worksheet.Cell(referralRow, i + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(referralRow, 15).Value =
                Enumerable.Range(7, 6).Sum(i => totalSourcesMonthly[i]);
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
                Enumerable.Range(7, 6).Sum(i => opdList.Count(opd => !opd.IsOld && opd.Date.Month == i));
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
                Enumerable.Range(7, 6).Sum(i => opdList.Count(opd => opd.IsOld && opd.Date.Month == i));
            worksheet.Cell(caseloadRow, 15).Style.Font.Bold = true;
            worksheet.Cell(caseloadRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            caseloadRow++;

            StatisticsHelper.ApplyWorksheetStatistics(worksheet, caseloadRow, "2. Closed Summary", 1, totalStatisticsMonthlyDictionary, "ii_ClosedSummary");

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
                Enumerable.Range(7, 6).Sum(i => opdList.Count(opd => opd.IsPWD && opd.Date.Month == i));
            worksheet.Cell(caseloadRow, 15).Style.Font.Bold = true;
            worksheet.Cell(caseloadRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            caseloadRow++;

            StatisticsHelper.ApplyWorksheetStatistics(worksheet, caseloadRow, "b. Indigenous People", 2, totalStatisticsMonthlyDictionary, "ii_NoPatients_IndigenousPeople");

            caseloadRow++;

            StatisticsHelper.ApplyWorksheetStatistics(worksheet, caseloadRow, "c. Government Workers", 2, totalStatisticsMonthlyDictionary, "ii_NoPatients_GovernmentWorkers");

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

            var ii_planning = new Dictionary<string, string>
            {
                { "1.1 Socio Economic Classification", "ii_Planning_SocioEconomicClassification"},
                { "1.2 Pre-admission Planning", "ii_Planning_PreAdmissionPlanning"},
                { "1.3 Information Services/Orientation", "ii_Planning_InformationServices"},
            };

            foreach (var key in ii_planning.Keys)
            {
                serviceRow++;

                StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, key, 1, totalStatisticsMonthlyDictionary, ii_planning[key]);
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

            StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, "2.1 Provision of Discount", 1, totalStatisticsMonthlyDictionary, "ii_Concrete_ProvisionDiscount");

            serviceRow++;

            StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, "2.2 Facilitating Referrals", 1, totalStatisticsMonthlyDictionary, "ii_Concrete_FacilitatingReferrals");

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

            var outgoingReferrals = new Dictionary<string, string>
            {
                { "a. Medical Assistance", "ii_Concrete_OutgoingReferrals_MedicalAssistance" },
                { "b. Discount on Procedure/Hospital Expenses", "ii_Concrete_OutgoingReferrals_DiscountProcedure" },
                { "c. Transportation Fare", "ii_Concrete_OutgoingReferrals_TransportationFare" },
                { "d. Institutional Placement", "ii_Concrete_OutgoingReferrals_InstitutionalPlacement" },
                { "e. Temporary Shelter", "ii_Concrete_OutgoingReferrals_TemporaryShelter" },
                { "f. Funeral Assistance/Pauper's Burial", "ii_Concrete_OutgoingReferrals_FuneralAssistance" },
                { "g. Others specify (networking)", "ii_Concrete_OutgoingReferrals_OthersSpecify" }
            };

            foreach (var key in outgoingReferrals.Keys)
            {
                serviceRow++;

                StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, key, 2, totalStatisticsMonthlyDictionary, outgoingReferrals[key]);
            }

            serviceRow++;

            StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, "2.2.2 Incoming Referrals", 1, totalStatisticsMonthlyDictionary, "ii_Concrete_IncomingReferrals");

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

            var tenLeadingCauses = new Dictionary<string, string>
            {
                { "1. Stress of the family", "ii_Psychosocial_TenLeading_StressFamily" },
                { "2. Refusal of patient to take home", "ii_Psychosocial_TenLeading_RefusalPatientTakeHome" },
                { "3. Anxiety of health cost", "ii_Psychosocial_TenLeading_AnxietyHealth" },
                { "4. Marital problem", "ii_Psychosocial_TenLeading_MaritalProblem" },
                { "5. Refusal of patient for treatment", "ii_Psychosocial_TenLeading_RefusalPatientTreatment" },
                { "6. Unbecoming attitude due to postponement if surgery", "ii_Psychosocial_TenLeading_UnbecomingAttitude" },
                { "7. Emotional problem", "ii_Psychosocial_TenLeading_EmotionalProblem" },
                { "8. Neglected children", "ii_Psychosocial_TenLeading_NeglectedChildren" },
                { "9. Sexually abuse", "ii_Psychosocial_TenLeading_SexuallyAbuse" },
                { "10. Adjustment problem", "ii_Psychosocial_TenLeading_AdjustedProblem" }
            };

            foreach (var key in tenLeadingCauses.Keys)
            {
                serviceRow++;

                StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, key, 2, totalStatisticsMonthlyDictionary, tenLeadingCauses[key]);
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

            serviceRow++;

            StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, "a. Social Worker", 2, totalStatisticsMonthlyDictionary, "ii_Psychosocial_FamilyCounseling_SocialWorker");
            
            serviceRow++;

            StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, "b. Health Care Team", 2, totalStatisticsMonthlyDictionary, "ii_Psychosocial_FamilyCounseling_HealthCareTeam");

            var psychologicalCounseling = new Dictionary<string, string>
            {
                { "3.3 Psychosocial Crisis Intervention", "ii_Psychosocial_PsychosocialCrisis"},
                { "3.4 Group Work/Per Session", "ii_Psychosocial_GroupWork"},
                { "3.5 Patients/Watchers Education", "ii_Psychosocial_PatientsEducation"},
                { "3.6 Mutual Support Group Session", "ii_Psychosocial_MutualSupport"},
                { "3.7 Advocacy Group", "ii_Psychosocial_AdvocacyGroup"}
            };

            foreach (var key in psychologicalCounseling.Keys)
            {
                serviceRow++;

                StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, key, 1, totalStatisticsMonthlyDictionary, psychologicalCounseling[key]);
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

            var dischargesServices = new Dictionary<string, string>
            {
                { "a. Discharge Planning", "ii_Discharges_DischargePlanning" },
                { "b. Facilitation of Discharge", "ii_Discharges_FacilitationDischarge" },
                { "c. Pre-termination Counseling", "ii_Discharges_PreTerminationCounseling" },
                { "d. Home Conduction", "ii_Discharges_HomeConduction" }
            };

            foreach (var key in dischargesServices.Keys)
            {
                serviceRow++;

                StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, key, 1, totalStatisticsMonthlyDictionary, dischargesServices[key]);
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

            var wardVisitation = new Dictionary<string, string>
            {
                { "a. Individual", "ii_Support_Ward_Individual" },
                { "b. Team", "ii_Support_Ward_Team" }
            };

            foreach (var key in wardVisitation.Keys)
            {
                serviceRow++;

                StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, key, 2, totalStatisticsMonthlyDictionary, wardVisitation[key]);
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

            var caseConferences = new Dictionary<string, string>
            {
                { "a. Multi Disciplinary", "ii_Case_MultiDisciplinary" },
                { "b. MSWD", "ii_Case_MSWD" }
            };

            foreach (var key in caseConferences.Keys)
            {
                serviceRow++;

                StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, key, 1, totalStatisticsMonthlyDictionary, caseConferences[key]);
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

            var followUpServices = new Dictionary<string, string>
            {
                { "7.1 Home Visit", "ii_FollowUp_HomeVisit" },
                { "7.2 Letters Sent", "ii_FollowUp_LettersSent" },
                { "7.3 Contact of Relatives by Telephone", "ii_FollowUp_ContactRelativesTelephone" },
                { "7.4 Contact of Relatives through Mass Media", "ii_FollowUp_ContactRelativesMassMedia" }
            };

            foreach (var key in followUpServices.Keys)
            {
                serviceRow++;

                StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, key, 1, totalStatisticsMonthlyDictionary, followUpServices[key]);
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

            var coordinationsByMSW = new Dictionary<string, string>
            {
                { "a. Physicians", "ii_Coordination_Physicians" },
                { "b. Nurses", "ii_Coordination_Nurses" },
                { "c. Pharmacist", "ii_Coordination_Pharmacist" },
                { "d. Nutritionist", "ii_Coordination_Nutritionist" },
                { "e. Other Staff", "ii_Coordination_OtherStaff" },
                { "f. Management", "ii_Coordination_Management" }
            };

            foreach (var key in coordinationsByMSW.Keys)
            {
                serviceRow++;

                StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, key, 1, totalStatisticsMonthlyDictionary, coordinationsByMSW[key]);
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

            var consultatives = new Dictionary<string, string>
            {
                { "a. Physicians", "ii_Consultive_Physicians" },
                { "b. Office Staff", "ii_Consultive_OfficeStaff" },
                { "c. Outside Hospital", "ii_Consultive_OutsideHospital" },
            };

            foreach (var key in consultatives.Keys)
            {
                serviceRow++;

                StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, key, 1, totalStatisticsMonthlyDictionary, consultatives[key]);
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

            var caseManagements = new Dictionary<string, string>
            {
                { "1.1 Pre admission Counselling", "iii_PreAdmissionCounseling" },
                { "1.2 Intake Interview", "iii_IntakeInterview" },
                { "1.3 Collateral Interview", "iii_CollateralInterview" },
                { "1.4 Issuance of MSS Card", "iii_IssuanceMSSCard" },
                { "1.5 Indicate classification in the chart (in pts only)", "iii_IndicateClassification" },
                { "1.6 Psychosocial Assessment", "iii_PsychosocialAssessment" },
                { "1.7 Psychosocial Counselling", "iii_PsychosocialCounseling" },
                { "1.8 Coordination w/ Multidiciplinary Team", "iii_CoordinationMultidisciplinary" },
                { "1.9 Completion of Intake Form", "iii_CompletionIntakeForm" },
                { "1.10 Health Education", "iii_HealthEducation" },
                { "1.11 Crisis Intervention", "iii_CrisisIntervention" },
            };

            foreach (var key in caseManagements.Keys)
            {
                serviceRow++;

                StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, key, 1, totalStatisticsMonthlyDictionary, caseManagements[key]);
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "1.12 Concrete Services";
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

            var concreteServices = new Dictionary<string, string>
            {
                { "1.12.1 Facilitaion/Provision of Meds/Procedures", "iii_ConcreteServices_Facilitation" },
                { "1.12.2 Transportation Assistance (w/in MSS resources)", "iii_ConcreteServices_Transportation" },
                { "1.12.3 Material Assistance (food, clothing)", "iii_ConcreteServices_MaterialAssistance" },
                { "1.12.4 Financial Assistance (w/in MSS resources)", "iii_ConcreteServices_FinancialAssistance" }
            };

            foreach (var key in concreteServices.Keys)
            {
                serviceRow++;

                StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, key, 2, totalStatisticsMonthlyDictionary, concreteServices[key]);
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

            var referralServices = new Dictionary<string, string>
            {
                { "1.13.1 Facilitating Incoming Referral", "iii_Referral_Facilitating" },
                { "1.13.2 Preparing the Referral", "iii_Referral_Preparing" },
                { "1.13.3 Coordination w/ the Receiveing Agency", "iii_Referral_Coordination" }
            };

            foreach (var key in referralServices.Keys)
            {
                serviceRow++;

                StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, key, 2, totalStatisticsMonthlyDictionary, referralServices[key]);
            }

            var caseManagements2 = new Dictionary<string, string>
            {
                { "1.14 Ward Rounds (no. of pts visited)", "iii_WardRounds" },
                { "1.15 Home Visitation", "iii_HomeVisitation" },
                { "1.16 Advocacy Role", "iii_AdvocacyRole" },
                { "1.17 Education", "iii_Education" },
            };

            foreach (var key in caseManagements2.Keys)
            {
                serviceRow++;

                StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, key, 1, totalStatisticsMonthlyDictionary, caseManagements2[key]);
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "1.18 Therapeutic Social Work Services";
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

            var therapeuticServices = new Dictionary<string, string>
            {
                { "1.18.1 Abandoned", "iii_Therapeutic_Abandoned" },
                { "1.18.2 Sexually Abused", "iii_Therapeutic_SexuallyAbused" },
                { "1.18.3 Neglected", "iii_Therapeutic_Neglected" },
                { "1.18.4 Battered", "iii_Therapeutic_Battered" }
            };

            foreach (var key in therapeuticServices.Keys)
            {
                serviceRow++;

                StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, key, 2, totalStatisticsMonthlyDictionary, therapeuticServices[key]);
            }

            var caseManagements3 = new Dictionary<string, string>
            {
                { "1.19 Protective Services", "iii_ProtectiveServices" },
                { "1.20 Grief Work", "iii_GriefWork" },
                { "1.21 Behavioral Modification ", "iii_Behavioral" },
                { "1.22 Networking (meeting w/ other institution/grp org.)", "iii_Networking" },
                { "1.23 Politicians", "iii_Politicians" },
                { "1.24 Coordination w/ Mass Media", "iii_CoordinationMassMedia" },
                { "1.25 Consultaion/Advisory Services", "iii_ConsultationAdvisory" },
                { "1.26 Attendance to Case Conferences Committee Meetings", "iii_AttendanceCaseConference" },
            };

            foreach (var key in caseManagements3.Keys)
            {
                serviceRow++;

                StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, key, 1, totalStatisticsMonthlyDictionary, caseManagements3[key]);
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "1.27 Attendance to Clinical Comittees";
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

            var attendances = new Dictionary<string, string>
            {
                { "1.27.1 Disharge Planning", "iii_AttendanceClinical_Discharge" },
                { "1.27.2 Facilitation of Discharge", "iii_AttendanceClinical_Facilitation" },
                { "1.27.3 Home Conduction", "iii_AttendanceClinical_Home" }
            };

            foreach (var key in attendances.Keys)
            {
                serviceRow++;

                StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, key, 2, totalStatisticsMonthlyDictionary, attendances[key]);
            }

            serviceRow++;

            StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, "1.28 Follow up Services", 1, totalStatisticsMonthlyDictionary, "iii_FollowUpServices");

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "1.29 Documentation";
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

            var documentations = new Dictionary<string, string>
            {
                { "1.29.1 Profile", "iii_Documentation_Profile" },
                { "1.29.2 Progress Notes", "iii_Documentation_ProgressNotes" },
                { "1.29.3 Groupwork Recording", "iii_Documentation_GroupWork" },
                { "1.29.4 Social Case Study Report/Social Case Summary", "iii_Documentation_SocialCase" },
                { "1.29.5 Home Visit Report", "iii_Documentation_HomeVisit" }
            };

            foreach (var key in documentations.Keys)
            {
                serviceRow++;

                StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, key, 2, totalStatisticsMonthlyDictionary, documentations[key]);
            }

            var caseManagements5 = new Dictionary<string, string>
            {
                { "1.30 Palliative Care", "iii_Palliative" },
                { "1.31 Facilitation of Unclaimed Cadaver", "iii_FacilitationUnclaimed" },
                { "1.32 Post Discharge Services", "iii_PostDischarge" },
                { "1.33 Follow up Services through text/phone", "iii_FollowUpServicesText" },
                { "1.34 Follow up Treatment Plans", "iii_FollowUpTreatment" },
                { "1.35 Follow up of Rehabilitation Plans", "iii_FollowUpRehabilitation" },
            };

            foreach (var key in caseManagements5.Keys)
            {
                serviceRow++;

                StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, key, 1, totalStatisticsMonthlyDictionary, caseManagements5[key]);
            }

            serviceRow++;

            worksheet.Cell(serviceRow, 1).Value = "1.36 Rehabilitation Services";
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

            var rehabilitationServices = new Dictionary<string, string>
            {
                { "1.36.1 Skills Training", "iii_Rehabilitation_Skills" },
                { "1.36.2 Job Placement", "iii_Rehabilitation_Job" },
                { "1.36.3 Capital Assistance", "iii_Rehabilitation_Capital" }
            };

            foreach (var key in rehabilitationServices.Keys)
            {
                serviceRow++;

                StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, key, 2, totalStatisticsMonthlyDictionary, rehabilitationServices[key]);
            }

            var caseManagements6 = new Dictionary<string, string>
            {
                { "1.37 MSWD Fund Raising Activity", "iii_MSWDFund" },
                { "1.38 Hospital Activity", "iii_HospitalActivity" },
                { "1.39 Linkage w/ Donors", "iii_LinkageDonors" }
            };

            foreach (var key in caseManagements6.Keys)
            {
                serviceRow++;

                StatisticsHelper.ApplyWorksheetStatistics(worksheet, serviceRow, key, 1, totalStatisticsMonthlyDictionary, caseManagements6[key]);
            }

            serviceRow++;

            var totalCaseloadMonthly = new Dictionary<int, int>();
            for (int i = 1; i <= 12; i++)
            {
                totalCaseloadMonthly[i] =
                    opdList.Count(m => !m.IsOld && m.Date.Month == i) +
                    opdList.Count(m => m.IsOld && m.Date.Month == i) +
                    opdList.Count(m => m.IsPWD && m.Date.Month == i);
            }

            worksheet.Cell(serviceRow, 1).Value = "TOTAL";
            worksheet.Cell(serviceRow, 1).Style.Font.Bold = true;
            worksheet.Cell(serviceRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int i = 1; i <= 6; i++)
            {
                worksheet.Cell(serviceRow, i + 1).Value = totalCaseloadMonthly[i] + totalStatisticsMonthlyDictionary[i].Values.Sum();
                worksheet.Cell(serviceRow, i + 1).Style.Font.Bold = true;
                worksheet.Cell(serviceRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(serviceRow, 8).Value =
                Enumerable.Range(1, 6).Sum(i => totalCaseloadMonthly[i] + totalStatisticsMonthlyDictionary[i].Values.Sum());
            worksheet.Cell(serviceRow, 8).Style.Font.Bold = true;
            worksheet.Cell(serviceRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int i = 7; i <= 12; i++)
            {
                worksheet.Cell(serviceRow, i + 2).Value = totalCaseloadMonthly[i] + totalStatisticsMonthlyDictionary[i].Values.Sum();
                worksheet.Cell(serviceRow, i + 2).Style.Font.Bold = true;
                worksheet.Cell(serviceRow, i + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(serviceRow, 15).Value =
                Enumerable.Range(7, 6).Sum(i => totalCaseloadMonthly[i] + totalStatisticsMonthlyDictionary[i].Values.Sum());
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

        [HasPermission("EditOPD")]
        public async Task<IActionResult> EditStatistics(string? sortToggle)
        {
            string sortToggleValue = sortToggle ?? "01";

            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await context.Users.FirstOrDefaultAsync(u => u.UserID == int.Parse(userId));

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found";
                return RedirectToAction("Statistics");
            }

            // Parse selected month/year
            int monthNumber = int.TryParse(sortToggleValue, out var parsedMonth) ? parsedMonth : 1;
            int yearNumber = DateTime.Now.Year;

            ViewBag.sortToggle = monthNumber.ToString("D2"); // keep 2-digit format for the UI
            ViewBag.sortToggleText = new DateOnly(yearNumber, monthNumber, 1).ToString("MMMM");

            // Try to get existing statistics for this user, type, month & year
            var statistics = await context.Statistics.FirstOrDefaultAsync(s =>
                s.Type == "OPD" &&
                s.UserID == user.UserID &&
                s.Date.HasValue &&
                s.Date.Value.Month == monthNumber &&
                s.Date.Value.Year == yearNumber);

            // If not found, create new
            if (statistics == null)
            {
                statistics = new StatisticsModel
                {
                    Type = "OPD",
                    UserID = user.UserID,
                    Date = new DateOnly(yearNumber, monthNumber, 1)
                };

                await context.Statistics.AddAsync(statistics);
                await context.SaveChangesAsync();
            }

            // Get OPDs for the same user and same month/year
            var opdList = await context.OPD
                .Where(o => o.UserID == user.UserID &&
                            o.Date.Month == monthNumber &&
                            o.Date.Year == yearNumber)
                .ToListAsync();

            var model = new OPDViewModel
            {
                User = user,
                OPDList = opdList,
                Statistics = statistics
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission("EditOPD")]
        public async Task<IActionResult> EditStatistics(OPDViewModel viewModel)
        {
            try
            {
                string connectionString = _connectionService.GetCurrentConnectionString();
                await using var context = new ApplicationDbContext(connectionString);
                context.Statistics.Update(viewModel.Statistics);
                await context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully edited/updated Statistics for OPD: {viewModel.Statistics.UserID}";
                LoggingService.LogInformation($"UserID: {User.FindFirst(ClaimTypes.NameIdentifier).Value}. Statistics edited/updated successfully for OPD");
                return RedirectToAction("Statistics");
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
    }
}
