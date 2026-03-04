using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LittleArkFoundation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [HasPermission("ManageDSS")]
    public class CriteriaController : Controller
    {
        private readonly ConnectionService _connectionService;

        public CriteriaController(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<IActionResult> Index()
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var criteriaList = await context.Criteria.ToListAsync();
            var criteriaDisplayNamesList = await context.Criteria.Where(c => c.IsDisplayName).ToListAsync();

            var viewModel = new Models.Criteria.CriteriaViewModel
            {
                CriteriaList = criteriaList,
                CriteriaDisplayNamesList = criteriaDisplayNamesList,
            };

            return View(viewModel);
        }
    }
}
