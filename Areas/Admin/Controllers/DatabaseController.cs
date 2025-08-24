using DocumentFormat.OpenXml.Wordprocessing;
using LittleArkFoundation.Areas.Admin.Models.Database;
using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

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
            try
            {
                string originalDbName = _databaseService.GetSelectedDatabaseInConnectionString(_connectionService.GetDefaultConnectionString());
                string newDbName = await _databaseService.GenerateNewDatabaseNameAsync(originalDbName);

                var viewModel = new DatabaseViewModel
                {
                    DefaultConnectionString = _connectionService.GetDefaultConnectionString(),
                    DefaultDatabaseName = _databaseService.GetSelectedDatabaseInConnectionString(_connectionService.GetDefaultConnectionString()),
                    CurrentConnectionString = _connectionService.GetCurrentConnectionString(),
                    CurrentDatabaseName = _databaseService.GetSelectedDatabaseInConnectionString(_connectionService.GetCurrentConnectionString()),
                    Databases = await _databaseService.GetDatabaseConnectionStringsAsync(),
                    DatabaseBackupFileNames = new [] {
                        $"{originalDbName}",
                        $"{newDbName}"
                    },
                };

                return View(viewModel);
            }
            catch (SqlException ex)
            {
                LoggingService.LogError("SQL Error: " + ex);
                TempData["ErrorMessage"] = " SQL Error: " + ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission("CreateDatabase")]
        public async Task<IActionResult> CreateNewDatabaseYear(bool? isArchive)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                string originalDbName = _databaseService.GetSelectedDatabaseInConnectionString(_connectionService.GetDefaultConnectionString());
                string newDbName = await _databaseService.GenerateNewDatabaseNameAsync(originalDbName);
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss"); // Safe format
                string backupFilePath = $"{originalDbName}_{timestamp}_backup.bak";
                string newBackupFilePath = $"{newDbName}_{timestamp}_backup.bak";

                LoggingService.LogInformation($"Database creation attempt. Database: {newDbName}, UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");

                await _databaseService.BackupDatabaseAsync(backupFilePath, originalDbName);
                await _databaseService.RestoreDatabaseAsync(backupFilePath, newDbName);
                await _databaseService.BackupDatabaseAsync(newBackupFilePath, newDbName);

                if (isArchive.HasValue && isArchive.Value)
                {
                    await _databaseService.RemoveAllCurrentData();
                }

                TempData["SuccessMessage"] = $"Successfully created new database year {newDbName}.";
                LoggingService.LogInformation($"Database creation attempt successful. Database: {newDbName}, UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");
                return RedirectToAction("Index");
            }
            catch (SqlException ex)
            {
                LoggingService.LogError("SQL Error: " + ex);
                TempData["ErrorMessage"] = " SQL Error: " + ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public IActionResult Connect(string connectionString)
        {
            try
            {
                HttpContext.Session.Remove("ConnectionString");
                HttpContext.Session.Remove("DatabaseName");
                HttpContext.Session.SetString("ConnectionString", connectionString);
                HttpContext.Session.SetString("DatabaseName", _databaseService.GetSelectedDatabaseInConnectionString(connectionString));
                TempData["SuccessMessage"] = $"Successfully connected to database {_databaseService.GetSelectedDatabaseInConnectionString(connectionString)}.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HasPermission("BackupRestoreDatabase")]
        public async Task<IActionResult> Backup(string name)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                LoggingService.LogInformation($"Database backup attempt. Database: {name}, UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss"); // Safe format
                string backupFilePath = $"{name}_{timestamp}_backup.bak";
                await _databaseService.BackupDatabaseAsync(backupFilePath, name);

                TempData["SuccessMessage"] = $"Successfully backed up database {name}.";
                LoggingService.LogInformation($"Database backup attempt successful. Database: {name}, UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");
                return RedirectToAction("Index");
            }
            catch (SqlException ex)
            {
                LoggingService.LogError("SQL Error: " + ex);
                TempData["ErrorMessage"] = " SQL Error: " + ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HasPermission("BackupRestoreDatabase")]
        public async Task<IActionResult> Restore(string name)
        {
            //string backupPath = @"C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\Backup";
            string backupPath = await _databaseService.GetSqlBackupPathAsync();
            string searchPattern = $"{_databaseService.GetSelectedDatabaseInConnectionString(_connectionService.GetDefaultConnectionString())}*.bak"; 
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
        [HasPermission("BackupRestoreDatabase")]
        public async Task<IActionResult> Restore(string name, string backupFileName)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                LoggingService.LogInformation($"Database restore attempt. Database: {name}, BackupFileName: {backupFileName} UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");

                string originalDbName = _databaseService.GetSelectedDatabaseInConnectionString(_connectionService.GetDefaultConnectionString());
                string backupFilePath = $"{backupFileName}";
                await _databaseService.RestoreDatabaseAsync(backupFilePath, name);

                TempData["SuccessMessage"] = $"Successfully restored database {name}.";
                LoggingService.LogInformation($"Database restore attempt successful. Database: {name}, BackupFileName: {backupFileName} UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");
                return RedirectToAction("Index");
            }
            catch (SqlException ex)
            {
                LoggingService.LogError("SQL Error: " + ex);
                TempData["ErrorMessage"] = " SQL Error: " + ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Delete(string name)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                LoggingService.LogInformation($"Database delete attempt. Database: {name}, UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");

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

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss"); // Safe format
                string backupFilePath = $"{name}_{timestamp}_backup.bak";
                await _databaseService.BackupDatabaseAsync(backupFilePath, name);
                await _databaseService.DeleteDatabaseAsync(name);

                TempData["SuccessMessage"] = $"Successfully deleted database {name}.";
                LoggingService.LogInformation($"Database delete attempt successful. Database: {name}, UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");
                return RedirectToAction("Index");
            }
            catch (SqlException ex)
            {
                LoggingService.LogError("SQL Error: " + ex);
                TempData["ErrorMessage"] = " SQL Error: " + ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> ViewBackupFiles(string name)
        {
            string backupPath = await _databaseService.GetSqlBackupPathAsync();
            string searchPattern = $"{_databaseService.GetSelectedDatabaseInConnectionString(_connectionService.GetDefaultConnectionString())}*.bak";
            string[] files = Directory.GetFiles(backupPath, searchPattern);
            string[] fileNames = Array.ConvertAll(files, Path.GetFileName); // Extract only file names

            var viewModel = new DatabaseViewModel
            {
                DefaultDatabaseName = name,
                DatabaseBackupFiles = fileNames.ToList()
            };

            return View(viewModel);
        }
    }
}
