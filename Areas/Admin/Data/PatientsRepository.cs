using LittleArkFoundation.Data;
using Microsoft.EntityFrameworkCore;

namespace LittleArkFoundation.Areas.Admin.Data
{
    public class PatientsRepository
    {
        private readonly ConnectionService _connectionService;
        private readonly string _connectionString;

        public PatientsRepository(ConnectionService connectionService) 
        {
            _connectionService = connectionService;
        }

        public PatientsRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> GenerateID()
        {
            try
            {
                await using var context = new ApplicationDbContext(_connectionString);
                var list = await context.Patients.ToListAsync();
                return list.Count + 1;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
