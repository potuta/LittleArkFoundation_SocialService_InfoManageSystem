using LittleArkFoundation.Data;
using Microsoft.EntityFrameworkCore;

namespace LittleArkFoundation.Areas.Admin.Data
{
    public class AssessmentsRepository
    {
        private readonly ConnectionService _connectionService;
        private readonly string _connectionString;

        public AssessmentsRepository(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public AssessmentsRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> GenerateID(int id)
        {
            try
            {
                await using var context = new ApplicationDbContext(_connectionString);
                var count = await context.Assessments.CountAsync(a => a.PatientID == id);
                return count + 1;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
