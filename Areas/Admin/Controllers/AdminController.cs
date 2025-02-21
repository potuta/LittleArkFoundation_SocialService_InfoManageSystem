using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LittleArkFoundation.Data;
using System.Security.Claims;

namespace LittleArkFoundation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ConnectionService _connectionService;

        public AdminController(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public IActionResult SecurePage()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            string connectionString = _connectionService.GetConnectionString("main");

            using (var context = new ApplicationDbContext(connectionString))
            {
                var user = context.Users.Find(Convert.ToInt32(userIdClaim.Value));
                if (user == null) return NotFound();
                ViewBag.WelcomeMessage = $"Welcome {user.Username}!";
            };
            return View();
        }
    }
}