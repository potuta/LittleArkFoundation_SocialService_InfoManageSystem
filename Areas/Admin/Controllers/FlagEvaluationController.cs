using LittleArkFoundation.Areas.Admin.Models.FlagEvaluation;
using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace LittleArkFoundation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [HasPermission("ManageDSS")]
    public class FlagEvaluationController : Controller
    {
        private readonly ConnectionService _connectionService;

        public FlagEvaluationController(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<IActionResult> Index()
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var flagEvaluationList = await context.FlagEvaluation.ToListAsync();

            var viewModel = new FlagEvaluationViewModel
            {
                FlagEvaluationList = flagEvaluationList
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission("EditDSS")]
        public async Task<IActionResult> Edit(FlagEvaluationViewModel viewModel)
        {
            try
            {
                string connectionString = _connectionService.GetCurrentConnectionString();
                await using var context = new ApplicationDbContext(connectionString);

                context.FlagEvaluation.UpdateRange(viewModel.FlagEvaluationList);

                await context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Successfully edited Flag Evaluation";
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                LoggingService.LogInformation($"UserID: {userIdClaim.Value}. Flag Evaluation edited successfully.");

                return RedirectToAction("Index");
            }
            catch (SqlException ex)
            {
                LoggingService.LogError($"SQL Error: {ex}");
                TempData["ErrorMessage"] = "SQL Error: " + ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Error: {ex}");
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
