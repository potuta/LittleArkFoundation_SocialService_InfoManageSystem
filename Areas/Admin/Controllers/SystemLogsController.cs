using LittleArkFoundation.Areas.Admin.Models.SystemLogs;
using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// TODO: Implement logging for system logs
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

        // TODO: Add search logs
    }
}
