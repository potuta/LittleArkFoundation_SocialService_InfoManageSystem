using Microsoft.Data.SqlClient;
using LittleArkFoundation.Areas.Admin.Models;
using Microsoft.EntityFrameworkCore;
using LittleArkFoundation.Data;

namespace LittleArkFoundation.Areas.Admin.Data
{
    public class UsersRepository
    {
        public readonly ConnectionService _connectionService;
        public readonly string _connectionString;

        public UsersRepository(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public UsersRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> VerifyUserExist(int id, string password)
        {
            try
            {
                await using (var connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT PasswordHash, PasswordSalt FROM Users WHERE UserID = @UserID";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", id);
                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string storedHash = reader.GetString(reader.GetOrdinal("PasswordHash"));
                                string storedSalt = reader.GetString(reader.GetOrdinal("PasswordSalt"));

                                // Hash the entered password using the stored salt
                                string enteredHash = PasswordService.HashPassword(password, Convert.FromBase64String(storedSalt));

                                // Compare the hashes
                                return enteredHash == storedHash;
                            }
                        }
                    }
                }
            }catch(SqlException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }

            return false; // User not found or password mismatch
        }

        public async Task<UsersModel?> GetUserAsync(int id, string password)
        {
            try
            {
                bool userExist = await VerifyUserExist(id, password);

                if (!userExist)
                {
                    return null;
                }

                UsersModel user = null;
                await using (var connection = new SqlConnection(_connectionString))
                {
                    string query = $"SELECT * FROM Users WHERE UserID = @UserID";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", id);
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                user = new UsersModel
                                {
                                    UserID = reader.GetInt32(0),
                                    Username = reader.GetString(1),
                                    Email = reader.GetString(2),
                                    PasswordHash = reader.GetString(3),
                                    PasswordSalt = reader.GetString(4),
                                    RoleID = reader.GetInt32(6),
                                    CreatedAt = reader.GetDateTime(7)
                                };
                            }
                        }
                    }
                }

                return user;
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

        public async Task<int> GenerateUserIDAsync(int roleID)
        {
            try
            {
                int userID = roleID * 10000;

                await using (var context = new ApplicationDbContext(_connectionString))
                {
                    List<UsersModel> usersList = await context.Users
                        .Where(r => r.RoleID == roleID)
                        .ToListAsync();

                    List<UsersArchivesModel> usersArchivesList = await context.UsersArchives
                        .Where(r => r.RoleID == roleID)
                        .ToListAsync();

                    foreach (var user in usersList)
                    {
                        userID = Math.Max(userID, user.UserID);
                    }

                    foreach (var user in usersArchivesList)
                    {
                        userID = Math.Max(userID, user.UserID);
                    }

                    userID++;
                }

                return userID;
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
