using Microsoft.AspNetCore.Mvc;
using LittleArkFoundation.Data;

namespace LittleArkFoundation.Controllers
{
    public class BackupController : Controller
    {
        private readonly DatabaseBackupService _databaseBackupService;

        public BackupController(DatabaseBackupService databaseBackupService)
        {
            _databaseBackupService = databaseBackupService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBackup()
        {
            //string backupFilePath = Path.Combine("C:\\Backups", $"Backup_{DateTime.Now:yyyyMMddHHmmss}.bak");
            string backupFilePath = $"LittleArkFoundation_DB_backup_{DateTime.Now}.bak";
            bool success = await _databaseBackupService.BackupDatabaseAsync(backupFilePath);

            if (success)
            {
                //return Ok($"Backup created at {backupFilePath}");
                ViewBag.Message = "Backup completed successfully!";
            }
            else
            {
                //return BadRequest("Backup failed.");
                ViewBag.Message = "Backup failed.";
            }

            return View();
        }
    }
}
