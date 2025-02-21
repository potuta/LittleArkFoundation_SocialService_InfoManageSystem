using System.Diagnostics;
using LittleArkFoundation.Data;
using LittleArkFoundation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LittleArkFoundation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ConnectionService _connectionService;

        public HomeController(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<IActionResult> Index()
        {
            string connectionString = _connectionService.GetConnectionString("main");

            using (var _dbContext = new ApplicationDbContext(connectionString))
            {
                var bloodInventory = await _dbContext.BloodInventory.ToListAsync();
                var recentRequests = await _dbContext.BloodRequests.OrderByDescending(r => r.RequestDate).Take(5).ToListAsync();

                var viewModel = new HomeViewModel
                {
                    BloodInventory = bloodInventory,
                    RecentRequests = recentRequests
                };

                return View(viewModel);
            }
        }
    }
}
