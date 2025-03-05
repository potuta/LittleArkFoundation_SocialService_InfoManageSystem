using Microsoft.AspNetCore.Mvc;
using LittleArkFoundation.Data;
using LittleArkFoundation.Areas.Admin.Models.Database;
using LittleArkFoundation.Authorize;
using Microsoft.AspNetCore.Authorization;

// TODO: Add logs to all methods in database controller
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
                DefaultConnectionString = _connectionService.GetDefaultConnectionString(),
                DefaultDatabaseName = _databaseService.GetSelectedDatabaseInConnectionString(_connectionService.GetDefaultConnectionString()),
                CurrentConnectionString = _connectionService.GetCurrentConnectionString(),
                CurrentDatabaseName = _databaseService.GetSelectedDatabaseInConnectionString(_connectionService.GetCurrentConnectionString()),
                Databases = await _databaseService.GetDatabaseConnectionStringsAsync()
            };

            return View(viewModel);
        } 

        public async Task<IActionResult> CreateNewDatabaseYear()
        {
            string originalDbName = _databaseService.GetSelectedDatabaseInConnectionString(_connectionService.GetDefaultConnectionString());
            string newDbName = await _databaseService.GenerateNewDatabaseNameAsync(originalDbName);
            string backupFilePath = $"{originalDbName}_backup.bak";

            await _databaseService.BackupDatabaseAsync(backupFilePath, originalDbName);
            await _databaseService.RestoreDatabaseAsync(backupFilePath, newDbName);

            TempData["SuccessMessage"] = $"Successfully created new database year {newDbName}.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Connect(string connectionString)
        {
            HttpContext.Session.Remove("ConnectionString");
            HttpContext.Session.Remove("DatabaseName");
            HttpContext.Session.SetString("ConnectionString", connectionString);
            HttpContext.Session.SetString("DatabaseName", _databaseService.GetSelectedDatabaseInConnectionString(connectionString));
            TempData["SuccessMessage"] = $"Successfully connected to database {_databaseService.GetSelectedDatabaseInConnectionString(connectionString)}.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Backup(string name)
        {
            string backupFilePath = $"{name}_backup.bak";
            await _databaseService.BackupDatabaseAsync(backupFilePath, name);
            TempData["SuccessMessage"] = $"Successfully backed up database {name}.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Restore(string name)
        {
            string backupPath = @"C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\Backup";
            string searchPattern = "MSWD_DB*.bak"; 
            string[] files = Directory.GetFiles(backupPath, searchPattern);
            string[] fileNames = Array.ConvertAll(files, Path.GetFileName); // Extract only file names

            var viewModel = new DatabaseViewModel
            {
                DefaultDatabaseName = name,
                DatabaseBackupFiles = fileNames.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(string name, string backupFileName)
        {
            Console.WriteLine($"Restoring database {name} from backup file {backupFileName}.");
            string originalDbName = _databaseService.GetSelectedDatabaseInConnectionString(_connectionService.GetDefaultConnectionString());
            string backupFilePath = $"{backupFileName}";
            await _databaseService.RestoreDatabaseAsync(backupFilePath, name);
            TempData["SuccessMessage"] = $"Successfully restored database {name}.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(string name)
        {
            if (name == _databaseService.GetSelectedDatabaseInConnectionString(_connectionService.GetDefaultConnectionString()))
            {
                TempData["ErrorMessage"] = "Cannot delete the default database.";
                return RedirectToAction("Index");
            }

            if (name == _databaseService.GetSelectedDatabaseInConnectionString(_connectionService.GetCurrentConnectionString()))
            {
                TempData["ErrorMessage"] = "Cannot delete the database you're currently connected to. Please change to the default database.";
                return RedirectToAction("Index");
            }

            TempData["SuccessMessage"] = $"Successfully deleted database {name}.";
            await _databaseService.DeleteDatabaseAsync(name);
            return RedirectToAction("Index");
        }
    }
}
