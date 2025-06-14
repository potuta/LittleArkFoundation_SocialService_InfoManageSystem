using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using LittleArkFoundation.Areas.Admin.Models.Form;
using LittleArkFoundation.Areas.Admin.Models.OPD;
using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

        public async Task<IActionResult> Index(bool? isAdmitted)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            bool activeFlag = isAdmitted ?? false;
            ViewBag.isAdmitted = activeFlag;

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();
            var opdList = await context.OPD
                .Where(opd => opd.IsAdmitted == activeFlag)
                .ToListAsync();

            var viewModel = new OPDViewModel
            {
                OPDList = opdList,
                Users = users
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Search(string searchString, bool? isAdmitted)
        {
            bool activeFlag = isAdmitted ?? false;
            ViewBag.isAdmitted = activeFlag;

            if (string.IsNullOrEmpty(searchString))
            {
                // If no search string, return all patients with the specified active flag
                return RedirectToAction("Index", new {isAdmitted = activeFlag });
            }

            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var searchWords = searchString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var query = context.OPD.Where(u => u.IsAdmitted == activeFlag);

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

            var viewModel = new OPDViewModel
            {
                OPDList = opdList,
            };

            return View("Index", viewModel);
        }

        public async Task<IActionResult> SortBy(string sortByUserID)
        {
            if (string.IsNullOrEmpty(sortByUserID))
            {
                return RedirectToAction("Index");
            }

            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            int userId = int.Parse(sortByUserID);

            var opdList = await context.OPD.Where(opd => opd.UserID == userId).ToListAsync();
            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            var viewModel = new OPDViewModel
            {
                OPDList = opdList,
                Users = users
            };

            var user = await context.Users.FindAsync(userId);

            ViewBag.sortBy = user.Username;
            return View("Index", viewModel);
        }

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

        //public async Task<IActionResult> ExportLogsheetToExcel(int userID)
        //{

        //}

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
    }
}
