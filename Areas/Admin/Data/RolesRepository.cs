using Microsoft.Data.SqlClient;
using LittleArkFoundation.Data;
using LittleArkFoundation.Areas.Admin.Models;
using Microsoft.EntityFrameworkCore;

namespace LittleArkFoundation.Areas.Admin.Data
{
    public class RolesRepository
    {
        private readonly string _connectionString;
        private readonly ConnectionService _connectionService;

        public RolesRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public RolesRepository(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<IEnumerable<RolesModel>> GetRolesAsync()
        {
            try
            {
                string connectionString = _connectionService.GetCurrentConnectionString();
                await using (var context = new ApplicationDbContext(connectionString))
                {
                    return await Task.Run(() => context.Roles.ToListAsync());
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<string> GetRoleNameByRoleID(int roleID)
        {
            try
            {
                await using (var connection = new SqlConnection(_connectionString))
                {
                    string query = $"SELECT RoleName FROM Roles WHERE RoleID = @RoleID";
                    await using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RoleID", roleID);
                        await connection.OpenAsync();
                        var result = await command.ExecuteScalarAsync();
                        return (string)result;
                    }
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
        }

        public async Task<int> GenerateRoleIDAsync()
        {
            try
            {
                await using (var context = new ApplicationDbContext(_connectionString))
                {
                    List<RolesModel> roles = await context.Roles.ToListAsync();
                    return roles.Count + 1;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
