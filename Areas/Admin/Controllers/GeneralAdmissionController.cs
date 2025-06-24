using DocumentFormat.OpenXml.Office2010.Excel;
using LittleArkFoundation.Areas.Admin.Data;
using LittleArkFoundation.Areas.Admin.Models.GeneralAdmission;
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
    [HasPermission("ManageForm")]
    public class GeneralAdmissionController : Controller
    {
        private readonly ConnectionService _connectionService;
        public GeneralAdmissionController(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<IActionResult> Index()
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");
            var users = await context.Users.Where(u => u.RoleID == roleIDSocialWorker.RoleID).ToListAsync();

            var generalAdmissions = await context.GeneralAdmission.ToListAsync();

            var viewModel = new GeneralAdmissionViewModel
            {
                Users = users,
                GeneralAdmissions = generalAdmissions
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Search(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
            {
                // If no search string, return all patients with the specified active flag
                return RedirectToAction("Index");
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

        public async Task<IActionResult> SortBy(string sortByUserID, string? sortByMonth)
        {
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
    }
}
