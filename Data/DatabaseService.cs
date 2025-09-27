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

        public async Task<bool> RestoreDatabaseAsync(string backupFilePath, string newDbName)
        {
            try
            {
                string dataLogicalName = "";
                string logLogicalName = "";

                await using (var connection = new SqlConnection(_connectionService.GetCurrentConnectionString()))
                {
                    await connection.OpenAsync();

                    // Change database context to master
                    await using (var masterCommand = new SqlCommand("USE master;", connection))
                    {
                        await masterCommand.ExecuteNonQueryAsync();
                    }

                    // Step 1: Drop active connections
                    string dropConnectionsQuery = $@"
                        DECLARE @DatabaseName NVARCHAR(255) = '{newDbName}';
                        DECLARE @SQL NVARCHAR(MAX) = '';

                        SELECT @SQL = @SQL + 'KILL ' + CONVERT(VARCHAR(10), session_id) + ';'
                        FROM sys.dm_exec_sessions
                        WHERE database_id = DB_ID(@DatabaseName);

                        EXEC sp_executesql @SQL;
                        ";

                    await using (SqlCommand dropConnectionsCommand = new SqlCommand(dropConnectionsQuery, connection))
                    {
                        await dropConnectionsCommand.ExecuteNonQueryAsync();
                    }

                    // Step 2: Check if the database exists before altering
                    string checkDbExistsQuery = $@"
                        IF DB_ID('{newDbName}') IS NOT NULL
                            ALTER DATABASE [{newDbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";

                    await using (SqlCommand checkDbCommand = new SqlCommand(checkDbExistsQuery, connection))
                    {
                        await checkDbCommand.ExecuteNonQueryAsync();
                    }

                    // Step 3: Get Logical File Names from Backup
                    string fileListQuery = $"RESTORE FILELISTONLY FROM DISK = '{backupFilePath}'";

                    await using (SqlCommand fileListCommand = new SqlCommand(fileListQuery, connection))
                    await using (SqlDataReader reader = await fileListCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string logicalName = reader["LogicalName"].ToString();
                            string type = reader["Type"].ToString();

                            if (type == "D") dataLogicalName = logicalName;
                            if (type == "L") logLogicalName = logicalName;
                        }
                    }

                    if (string.IsNullOrEmpty(dataLogicalName) || string.IsNullOrEmpty(logLogicalName))
                        throw new Exception("Could not determine logical file names from backup.");

                    // Step 4: Restore Using Correct Logical Names
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

                    // Step 5: Set Database Back to MULTI_USER Mode
                    string setMultiUserQuery = $"ALTER DATABASE [{newDbName}] SET MULTI_USER;";
                    await using (SqlCommand multiUserCommand = new SqlCommand(setMultiUserQuery, connection))
                    {
                        await multiUserCommand.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
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

        public async Task<string> GetSqlBackupPathAsync()
        {
            try
            {
                string backupPath = "";

                await using (var connection = new SqlConnection(_connectionService.GetDefaultConnectionString()))
                {
                    connection.Open();
                    await using (var command = new SqlCommand(@"
                                DECLARE @BackupPath NVARCHAR(500)
                                EXEC master.dbo.xp_instance_regread 
                                    N'HKEY_LOCAL_MACHINE', 
                                    N'SOFTWARE\Microsoft\MSSQLServer\MSSQLServer', 
                                    N'BackupDirectory', 
                                    @BackupPath OUTPUT
                                SELECT @BackupPath AS BackupFolderPath", connection))
                    {
                        var result = await command.ExecuteScalarAsync();
                        if (result != null)
                        {
                            backupPath = result.ToString();
                        }
                    }
                }

                return backupPath;
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

        public async Task<bool> DeleteDatabaseAsync(string databaseName)
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
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Dictionary<string, string>> GetDatabaseConnectionStringsAsync()
        {
            Dictionary<string, string> databases = new Dictionary<string, string>();
            List<string> databaseNamesList = new List<string>();

            string query = @$"SELECT name
                            FROM sys.databases
                            WHERE (name NOT IN ('master', 'tempdb', 'model', 'msdb')) 
                            AND (name LIKE '%{GetSelectedDatabaseInConnectionString(_connectionService.GetDefaultConnectionString())}%')";

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
            }
            catch (Exception ex)
            {
                throw;
            }
            
            return databases;
        }

        public async Task<string> GetNewConnectionStringAsync(string databaseName)
        {
            var getConnectionString = new SqlConnectionStringBuilder(_connectionService.GetDefaultConnectionString())
            {
                InitialCatalog = databaseName
            }.ToString();

            return getConnectionString;
        }

        public async Task<string> GenerateNewDatabaseNameAsync(string originalDbName)
        {
            try
            {
                Dictionary<string, string> databases = await GetDatabaseConnectionStringsAsync();
                List<int> dbYearList = new List<int>();

                foreach (string name in databases.Keys)
                {
                    if (name.Contains("Temp", StringComparison.OrdinalIgnoreCase))
                    {
                        continue; // Skip temporary databases
                    }

                    // Get the year (the last part)
                    if (name.Contains("2"))
                    {
                        string[] nameParts = name.Split('_');
                        dbYearList.Add(Convert.ToInt32(nameParts[2]));
                    }
                    else
                    {
                        // This is the base database like "MSWD_DB"
                        dbYearList.Add(DateTime.Now.Year);
                    }
                }

                string year;

                if (databases.Count == 1)
                {
                    // Only "MSWD_DB" exists — need to create MSWD_DB_Temp
                    year = $"Temp";
                }
                else if (databases.Count == 2)
                {
                    // Only base and Temp exist — now introduce first year (current year)
                    year = $"{dbYearList.Max()}";
                }
                else
                {
                    // Existing years found — get max and increment
                    year = $"{dbYearList.Max() + 1}";
                }

                string newDbName = $"{originalDbName}_{year}";
                return newDbName;
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

        public string GetSelectedDatabaseInConnectionString(string connectionString)
        {
            try
            {
                var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
                string InitialCatalog = connectionStringBuilder.InitialCatalog;
                return InitialCatalog;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> RemoveAllCurrentData()
        {
            try
            {
                await using var connection = new SqlConnection(_connectionService.GetDefaultConnectionString());
                await connection.OpenAsync();

                // Executes each command separately to handle errors more clearly
                var commands = new[]
                {
                    "DELETE FROM Patients;",
                    "DBCC CHECKIDENT ('Patients', RESEED, 0);",
                    "TRUNCATE TABLE GeneralAdmission;",
                    "TRUNCATE TABLE Discharges;",
                    "TRUNCATE TABLE OPD;",
                    "TRUNCATE TABLE Logs;",
                    "TRUNCATE TABLE Stats;"
                };

                foreach (var query in commands)
                {
                    await using var command = new SqlCommand(query, connection);
                    await command.ExecuteNonQueryAsync();
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
    }

}
