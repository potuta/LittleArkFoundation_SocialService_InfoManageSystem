using Microsoft.AspNetCore.Mvc;
using LittleArkFoundation.Data;
using LittleArkFoundation.Areas.Admin.Models.Database;
using LittleArkFoundation.Authorize;
using Microsoft.AspNetCore.Authorization;


namespace LittleArkFoundation.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin")]
    [HasPermission("ManageDatabase")]
    public class DatabaseController : Controller
    {
        private readonly DatabaseService _databaseService;
        private readonly ConnectionService _connectionService;

        public DatabaseController(DatabaseService databaseService, ConnectionService connectionService)
        {
            _databaseService = databaseService;
            _connectionService = connectionService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DatabaseViewModel
            {
                Databases = await _databaseService.GetDatabaseConnectionStringsAsync()
            };

            ViewBag.CurrentConnectionString = _connectionService.GetCurrentConnectionString();
            ViewBag.DefaultConnectionString = _connectionService.GetDefaultConnectionString();
            return View(viewModel);
        } 

        public async Task<IActionResult> Connect(string connectionString)
        {
            HttpContext.Session.SetString("ConnectionString", connectionString);
            HttpContext.Session.SetString("DatabaseName", _databaseService.GetSelectedDatabaseInConnectionString(connectionString));
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> CreateNewDatabaseYear()
        {
            string originalDbName = _databaseService.GetSelectedDatabaseInConnectionString(_connectionService.GetDefaultConnectionString());
            string newDbName = await _databaseService.GenerateNewDatabaseNameAsync(originalDbName);
            string backupFilePath = $"{originalDbName}_backup.bak";

            await _databaseService.BackupDatabaseAsync(backupFilePath, originalDbName);
            await _databaseService.RestoreDatabaseAsync(backupFilePath, originalDbName, newDbName);

            return RedirectToAction("Index");
        }
    }
}
