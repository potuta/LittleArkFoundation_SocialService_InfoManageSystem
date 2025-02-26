using LittleArkFoundation.Areas.Admin.Models;
using LittleArkFoundation.Data;
using Microsoft.Data.SqlClient;

namespace LittleArkFoundation.Areas.Admin.Data
{
    public class PermissionsRepository
    {
        private readonly ConnectionService _connectionService;
        private readonly string _connectionString;

        public PermissionsRepository(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public PermissionsRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<string>> GetPermissionsByRoleID(int roleID)
        {
            try
            {
                var permissions = new List<string>();

                await using (var connection = new SqlConnection(_connectionString))
                {

                    string query = @"
                    SELECT P.Name 
                    FROM RolePermissions RP
                    INNER JOIN Permissions P ON RP.PermissionID = P.PermissionID
                    WHERE RP.RoleID = @RoleID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RoleID", roleID);
                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                permissions.Add(reader["Name"].ToString());
                            }
                        }
                    }
                }

                return permissions;
            }
            catch(SqlException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> GenerateID()
        {
            try
            {
                await using (var context = new ApplicationDbContext(_connectionString))
                {
                    List<PermissionsModel> permissions = context.Permissions.ToList();
                    return permissions.Count + 1;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
