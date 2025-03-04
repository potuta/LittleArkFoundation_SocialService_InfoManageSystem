using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace LittleArkFoundation.Data
{
    public class DatabaseService
    {
        private readonly ConnectionService _connectionService;

        public DatabaseService(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<bool> BackupDatabaseAsync(string backupFilePath, string originalDbName)
        {
            try
            {
                await using (var connection = new SqlConnection(_connectionService.GetCurrentConnectionString()))
                {
                    await connection.OpenAsync();
                    string backupQuery = $"BACKUP DATABASE [{originalDbName}] TO DISK = '{backupFilePath}' WITH INIT";
                    await using (var command = new SqlCommand(backupQuery, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (SqlException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> RestoreDatabaseAsync(string backupFilePath, string originalDbName, string newDbName)
        {
            try
            {
                string dataLogicalName = "";
                string logLogicalName = "";

                await using (var connection = new SqlConnection(_connectionService.GetCurrentConnectionString()))
                {
                    await connection.OpenAsync();

                    // Step 1: Get Logical File Names from Backup
                    string fileListQuery = $"RESTORE FILELISTONLY FROM DISK = '{backupFilePath}'";

                    await using (SqlCommand fileListCommand = new SqlCommand(fileListQuery, connection))
                    await using (SqlDataReader reader = await fileListCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string logicalName = reader["LogicalName"].ToString();
                            string type = reader["Type"].ToString();

                            if (type == "D") dataLogicalName = logicalName; // Data file
                            if (type == "L") logLogicalName = logicalName;  // Log file
                        }
                    }

                    if (string.IsNullOrEmpty(dataLogicalName) || string.IsNullOrEmpty(logLogicalName))
                        throw new Exception("Could not determine logical file names from backup.");

                    // Step 2: Restore Using Correct Logical Names
                    string restoreQuery = $@"
                        RESTORE DATABASE [{newDbName}]
                        FROM DISK = '{backupFilePath}'
                        WITH MOVE '{dataLogicalName}' TO '{await GetSqlDefaultDataPathAsync()}\{newDbName}.mdf',
                        MOVE '{logLogicalName}' TO '{await GetSqlDefaultDataPathAsync()}\{newDbName}_log.ldf',
                        REPLACE, RECOVERY";

                    await using (SqlCommand restoreCommand = new SqlCommand(restoreQuery, connection))
                    {
                        await restoreCommand.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (SqlException ex)
            {
                // Log error for debugging
                Console.WriteLine($"SQL Error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                // Log error for debugging
                Console.WriteLine($"General Error: {ex.Message}");
                throw;
            }
        }


        public async Task<string> GetSqlDefaultDataPathAsync()
        {
            string defaultDataPath = string.Empty;
            string defaultDataDirectory = string.Empty;

            try
            {
                await using (var connection = new SqlConnection(_connectionService.GetDefaultConnectionString()))
                {
                    await connection.OpenAsync();

                    // SQL to get the default SQL Server data directory
                    string query = @"
                        DECLARE @default_data_path NVARCHAR(256);
                        SET @default_data_path = 
                            (SELECT physical_name FROM sys.master_files 
                            WHERE type = 0 AND database_id = 1);
                        SELECT @default_data_path;";

                    await using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        defaultDataPath = await command.ExecuteScalarAsync() as string;
                    }
                }

                defaultDataDirectory = Path.GetDirectoryName(defaultDataPath);
            }
            catch (SqlException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }

            return defaultDataDirectory;
        }

        public async Task<bool> DeleteDatabase(string databaseName)
        {
            try
            {
                LoggingService.LogInformation($"Database deletion attempt in DeleteDatabase: DBName: {databaseName}");

                await using (var connection = new SqlConnection(_connectionService.GetDefaultConnectionString()))
                {
                    await connection.OpenAsync();

                    string query = $@"
                        ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                        DROP DATABASE [{databaseName}];";

                    await using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                        Console.WriteLine($"Database '{databaseName}' deleted successfully.");
                    }
                }

                LoggingService.LogInformation($"Database deletion succesful in DeleteDatabase: DBName: {databaseName}");
                return true;
            }
            catch (SqlException ex)
            {
                throw;
                Console.WriteLine($"SQL Error in DeleteDatabase: {ex.Message}");
                LoggingService.LogError($"SQL Error in DeleteDatabase: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw;
                Console.WriteLine($"Unexpected error in DeleteDatabase: {ex.Message}");
                LoggingService.LogError($"Unexpected error in DeleteDatabase: {ex.Message}");
            }
        }

        public async Task<Dictionary<string, string>> GetDatabaseConnectionStringsAsync()
        {
            Dictionary<string, string> databases = new Dictionary<string, string>();
            List<string> databaseNamesList = new List<string>();

            string query = @"SELECT name
                            FROM sys.databases
                            WHERE (name NOT IN ('master', 'tempdb', 'model', 'msdb')) 
                            AND (name LIKE '%LittleArkFoundation_SocialService_DB%')";

            try
            {
                await using (var connection = new SqlConnection(_connectionService.GetDefaultConnectionString()))
                {
                    await connection.OpenAsync();
                    await using (var command = new SqlCommand(query, connection))
                    {
                        await using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                for (int column = 0; column < reader.FieldCount; column++)
                                {
                                    databaseNamesList.Add(reader.GetString(column));
                                }
                            }
                        }
                    }
                }

                foreach (string name in databaseNamesList)
                {
                    databases[name] = await GetNewConnectionStringAsync(name);
                }
            }
            catch (SqlException ex)
            {
                throw;
                Console.WriteLine($"SQL Error in GetDatabaseConnectionStrings: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw;
                Console.WriteLine($"Unexpected error in GetDatabaseConnectionStrings: {ex.Message}");
            }
            
            return databases;
        }

        public async Task<string> GetNewConnectionStringAsync(string databaseName)
        {
            var getConnectionString = new SqlConnectionStringBuilder(_connectionService.GetDefaultConnectionString())
            {
                InitialCatalog = databaseName
            }.ToString();

            //var connectionString = new SqlConnection(getConnectionString);

            return getConnectionString;
        }

        public async Task<string> GenerateNewDatabaseNameAsync(string originalDbName)
        {
            Dictionary<string, string> databases = await GetDatabaseConnectionStringsAsync();
            List<int> dbPreviousYearList = new List<int>();
            List<int> dbNextYearList = new List<int>();
            string newYear = string.Empty;

            foreach (string name in databases.Keys)
            {
                if (name.Contains("2"))
                {
                    string[] nameParts = name.Split('_');
                    dbPreviousYearList.Add(Convert.ToInt32(nameParts[2]));
                    dbNextYearList.Add(Convert.ToInt32(nameParts[3]));
                }
                else
                {
                    dbPreviousYearList.Add(DateTime.Now.Year - 1);
                    dbNextYearList.Add(DateTime.Now.Year);
                }
            }

            if (databases.Count == 1)
            {
                newYear = $"{dbPreviousYearList.Max()}_{dbNextYearList.Max()}";
            }
            else
            {
                newYear = $"{dbPreviousYearList.Max() + 1}_{dbNextYearList.Max() + 1}";
            }

            string newDbName = $"{originalDbName}_{newYear}";
            return newDbName;
        }

        public string GetSelectedDatabaseInConnectionString(string connectionString)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            string InitialCatalog = connectionStringBuilder.InitialCatalog;
            return InitialCatalog;
        }

        // TODO: add truncate database method

        public async Task<bool> TruncateDatabaseAsync(string databaseName)
        {
            return false;
        }
    }

}
