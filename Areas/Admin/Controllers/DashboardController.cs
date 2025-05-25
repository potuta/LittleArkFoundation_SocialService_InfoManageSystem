using LittleArkFoundation.Areas.Admin.Models.Dashboard;
using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LittleArkFoundation.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin")]
    [HasPermission("ViewAdminDashboard")]
    public class DashboardController : Controller
    {
        private readonly ConnectionService _connectionService;

        public DashboardController(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<IActionResult> Index()
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);

            // Assessments
            var assessments = await context.Assessments
                .Where(a => a.UserID == userId)
                .ToListAsync();

            var dailyassessments = assessments
                .Where(a => a.DateOfInterview == DateOnly.FromDateTime(DateTime.Now))
                .ToList();

            var monthlyassessments = assessments
                .Where(a => a.DateOfInterview.Month == DateTime.Now.Month && a.DateOfInterview.Year == DateTime.Now.Year)
                .ToList();

            var yearlyassessments = assessments
                .Where(a => a.DateOfInterview.Year == DateTime.Now.Year)
                .ToList();

            // Discharges
            var discharges = await context.Discharges
                .Where(d => d.UserID == userId)
                .ToListAsync();

            var dailydischarges = discharges
                .Where(d => d.DischargedDate == DateOnly.FromDateTime(DateTime.Now))
                .ToList();

            var monthlydischarges = discharges
                .Where(d => d.DischargedDate.Month == DateTime.Now.Month && d.DischargedDate.Year == DateTime.Now.Year)
                .ToList();

            var yearlydischarges = discharges
                .Where(d => d.DischargedDate.Year == DateTime.Now.Year)
                .ToList();

            var viewModel = new DashboardViewModel()
            {
                DailyAssessments = dailyassessments,
                MonthlyAssessments = monthlyassessments,
                YearlyAssessments = yearlyassessments,
                DailyDischarges = dailydischarges,
                MonthlyDischarges = monthlydischarges,
                YearlyDischarges = yearlydischarges
            };

            return View(viewModel);
        }
    }
}
