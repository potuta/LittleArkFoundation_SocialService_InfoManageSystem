using LittleArkFoundation.Areas.Admin.Models.Form;
using LittleArkFoundation.Areas.Admin.Models.OPD;
using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LittleArkFoundation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [HasPermission("ManageOPD")]
    public class OPDController : Controller
    {
        private readonly ConnectionService _connectionService;
        public OPDController(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<IActionResult> Index(bool? isAdmitted)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            bool activeFlag = isAdmitted ?? false;
            ViewBag.isAdmitted = activeFlag;

            var opdList = await context.OPD
                .Where(opd => opd.IsAdmitted == activeFlag)
                .ToListAsync();

            var viewModel = new OPDViewModel
            {
                OPDList = opdList,
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Create()
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.UserID == int.Parse(userIdClaim.Value));

            var viewModel = new OPDViewModel
            {
                User = user
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OPDViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            await context.OPD.AddAsync(viewModel.OPD);
            await context.SaveChangesAsync();

            return View(viewModel);
        }
    }
}
