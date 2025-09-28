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
    [HasPermission("ManageDashboard")]
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

            // OPD
            var opdList = await context.OPD
                .Where(o => o.UserID == userId)
                .ToListAsync();

            var dailyOPD = opdList
                .Where(o => o.Date == DateOnly.FromDateTime(DateTime.Now))
                .ToList();

            var monthlyOPD = opdList
                .Where(o => o.Date.Month == DateTime.Now.Month && o.Date.Year == DateTime.Now.Year)
                .ToList();

            var yearlyOPD = opdList
                .Where(o => o.Date.Year == DateTime.Now.Year)
                .ToList();

            // General Admission
            var generalAdmissions = await context.GeneralAdmission
                .Where(a => a.UserID == userId)
                .ToListAsync();

            var dailyGA = generalAdmissions
                .Where(a => a.Date == DateOnly.FromDateTime(DateTime.Now))
                .ToList();

            var monthlyGA = generalAdmissions
                .Where(a => a.Date.Month == DateTime.Now.Month && a.Date.Year == DateTime.Now.Year)
                .ToList();

            var yearlyGA = generalAdmissions
                .Where(a => a.Date.Year == DateTime.Now.Year)
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

            // Progress Notes
            var progressNotes = await context.ProgressNotes
                .Where(p => p.UserID == userId)
                .ToListAsync();

            var dailyProgressNotes = progressNotes
                .Where(p => p.Date == DateOnly.FromDateTime(DateTime.Now))
                .ToList();

            var monthlyProgressNotes = progressNotes
                .Where(p => p.Date.Month == DateTime.Now.Month && p.Date.Year == DateTime.Now.Year)
                .ToList();

            var yearlyProgressNotes = progressNotes
                .Where(p => p.Date.Year == DateTime.Now.Year)
                .ToList();

            // ALL
            var opdAll = await context.OPD.ToListAsync();
            var generalAdmissionsAll = await context.GeneralAdmission.ToListAsync();
            var dischargesAll = await context.Discharges.ToListAsync();
            var progressNotesAll = await context.ProgressNotes.ToListAsync();

            var viewModel = new DashboardViewModel()
            {
                DailyOPD = dailyOPD,
                MonthlyOPD = monthlyOPD,
                YearlyOPD = yearlyOPD,
                DailyGA = dailyGA,
                MonthlyGA = monthlyGA,
                YearlyGA = yearlyGA,
                DailyDischarges = dailydischarges,
                MonthlyDischarges = monthlydischarges,
                YearlyDischarges = yearlydischarges,
                DailyProgressNotes = dailyProgressNotes,
                MonthlyProgressNotes = monthlyProgressNotes,
                YearlyProgressNotes = yearlyProgressNotes,
                OPDList = opdAll,
                GeneralAdmissionsList = generalAdmissionsAll,
                DischargesList = dischargesAll,
                ProgressNotesList = progressNotesAll
            };

            return View(viewModel);
        }
    }
}
