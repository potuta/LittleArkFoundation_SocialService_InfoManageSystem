using Microsoft.Data.SqlClient;

namespace LittleArkFoundation.Data
{
    public class DatabaseBackupService
    {
        private readonly string _connectionString;

        public DatabaseBackupService(IConfiguration configuration)
        {
            _connectionString = new ConnectionService(configuration).GetConnectionString("main");
        }

        public async Task<bool> BackupDatabaseAsync(string backupPath)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string backupQuery = $"BACKUP DATABASE [LittleArkFoundation_DB] TO DISK = '{backupPath}' WITH INIT";
                    using (var command = new SqlCommand(backupQuery, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Backup failed: {ex.Message}");
                return false;
            }
        }
    }

}
