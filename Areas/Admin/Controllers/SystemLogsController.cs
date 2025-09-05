using LittleArkFoundation.Areas.Admin.Models.GeneralAdmission;
using LittleArkFoundation.Areas.Admin.Models.SystemLogs;
using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using LittleArkFoundation.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LittleArkFoundation.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin")]
    [HasPermission("ManageSystemLogs")]
    public class SystemLogsController : Controller
    {
        private readonly ConnectionService _connectionService;
        private readonly IHubContext<LogsHub> _hubContext;
        
        public SystemLogsController(ConnectionService connectionService, IHubContext<LogsHub> hubContext)
        {
            _connectionService = connectionService;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index(string? sortByMonth, int page = 1, int pageSize = 20)
        {
            try
            {
                string connectionString = _connectionService.GetCurrentConnectionString();
                await using var context = new ApplicationDbContext(connectionString);

                var query = context.Logs.AsQueryable();

                if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime dateTime))
                {
                    query = query.Where(d => d.TimeStamp.Month == dateTime.Month && d.TimeStamp.Year == dateTime.Year && d.TimeStamp.Day == dateTime.Day);
                    ViewBag.sortByMonth = dateTime.ToString("yyyy-MM-dd");
                }

                var totalCount = await query.CountAsync();
                var logs = await query
                    .OrderByDescending(l => l.TimeStamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var logsViewModel = new LogsViewModel
                {
                    LogsList = logs,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
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

        public async Task<IActionResult> Search(string searchString, string? level, int page = 1, int pageSize = 20)
        {
            try
            {
                string connectionString = _connectionService.GetCurrentConnectionString();
                await using var context = new ApplicationDbContext(connectionString);

                if (string.IsNullOrEmpty(searchString))
                {
                    return RedirectToAction("Index");
                }

                var query = context.Logs
                    .Where(l => string.IsNullOrEmpty(searchString) ||
                    l.TimeStamp.Date == DateTime.Parse(searchString).Date);

                if (!string.IsNullOrEmpty(level))
                {
                    query = query.Where(d => d.Level.Equals(level));
                    ViewBag.sortBy = level;
                }

                var totalCount = await query.CountAsync();
                var logs = await query
                    .OrderByDescending(l => l.TimeStamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var logsViewModel = new LogsViewModel
                {
                    LogsList = logs,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };

                ViewBag.sortByMonth = searchString;

                return View("Index", logsViewModel);
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return View("Index");
            }
        }

        public async Task<IActionResult> SortBy(string? level, string? sortByMonth, string? viewName = "Index", int page = 1, int pageSize = 20)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var query = context.Logs.AsQueryable();

            if (!string.IsNullOrEmpty(level))
            {
                query = query.Where(d => d.Level.Equals(level));
                ViewBag.sortBy = level;
            }

            if (!string.IsNullOrWhiteSpace(sortByMonth) && DateTime.TryParse(sortByMonth, out DateTime dateTime))
            {
                query = query.Where(d => d.TimeStamp.Month == dateTime.Month && d.TimeStamp.Year == dateTime.Year && d.TimeStamp.Day == dateTime.Day);
                ViewBag.sortByMonth = dateTime.ToString("yyyy-MM-dd");
            }

            var totalCount = await query.CountAsync();
            var logs = await query
                .OrderByDescending(l => l.TimeStamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var roleIDSocialWorker = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Social Worker");

            var viewModel = new LogsViewModel
            {
                LogsList = logs,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
            };

            return View(viewName, viewModel);
        }
    }
}
