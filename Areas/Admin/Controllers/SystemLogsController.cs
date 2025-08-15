using LittleArkFoundation.Areas.Admin.Models.GeneralAdmission;
using LittleArkFoundation.Areas.Admin.Models.SystemLogs;
using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LittleArkFoundation.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin")]
    [HasPermission("ManageSystemLogs")]
    public class SystemLogsController : Controller
    {
        private readonly ConnectionService _connectionService;
        
        public SystemLogsController(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                string connectionString = _connectionService.GetCurrentConnectionString();

                await using var context = new ApplicationDbContext(connectionString);

                var logs = await context.Logs.OrderByDescending(l => l.TimeStamp).ToListAsync();

                var logsViewModel = new LogsViewModel
                {
                    LogsList = logs
                };

                return View(logsViewModel);
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return View("Index");
            }
        }

        public async Task<IActionResult> Search(string searchString)
        {
            try
            {
                string connectionString = _connectionService.GetCurrentConnectionString();
                await using var context = new ApplicationDbContext(connectionString);

                if (string.IsNullOrEmpty(searchString))
                {
                    return RedirectToAction("Index");
                }

                var logs = await context.Logs
                    .Where(l => string.IsNullOrEmpty(searchString) || 
                    l.TimeStamp.Date == DateTime.Parse(searchString).Date)
                    .ToListAsync();

                var logsViewModel = new LogsViewModel
                {
                    LogsList = logs
                };

                return View("Index", logsViewModel);
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return View("Index");
            }
        }

        public async Task<IActionResult> SortBy(string? level, string? sortByMonth, string? viewName = "Index")
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var query = context.Logs.AsQueryable();

            if (!string.IsNullOrEmpty(level))
            {
                query = query.Where(d => d.Level.Equals(level));
                ViewBag.sortBy = level;
            }

            if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime month))
            {
                query = query.Where(d => d.TimeStamp.Month == month.Month && d.TimeStamp.Year == month.Year);
                ViewBag.sortByMonth = month.ToString("yyyy-MM");
            }

            var logs = await query.OrderByDescending(l => l.TimeStamp).ToListAsync();

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");

            var viewModel = new LogsViewModel
            {
                LogsList = logs
            };

            return View(viewName, viewModel);
        }
    }
}
