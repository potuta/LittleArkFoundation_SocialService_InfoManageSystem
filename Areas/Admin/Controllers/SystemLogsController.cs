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

                var logs = await context.Logs.ToListAsync();

                var logsViewModel = new LogsViewModel
                {
                    LogsList = logs
                };

                return View(logsViewModel);
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex.Message);
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
                LoggingService.LogError("Error: " + ex.Message);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return View("Index");
            }
        }
    }
}
