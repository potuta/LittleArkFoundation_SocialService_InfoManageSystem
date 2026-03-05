using LittleArkFoundation.Areas.Admin.Models.AgeModifier;
using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LittleArkFoundation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [HasPermission("ManageDSS")]
    public class AgeModifierController : Controller
    {
        private readonly ConnectionService _connectionService;

        public AgeModifierController(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<IActionResult> Index()
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var ageModifiers = await context.AgeModifier.ToListAsync();
            var agesList = new List<(string age, int modifier)>();
            agesList.Add(("< " + ageModifiers[0].Age.ToString() + " Year", ageModifiers[0].Modifier));
            agesList.Add(($"{ageModifiers[1].Age.ToString()} - {ageModifiers[2].Age.ToString()} Years", ageModifiers[1].Modifier));
            agesList.Add((">= " + ageModifiers[3].Age.ToString() + " Year", ageModifiers[3].Modifier));

            var viewModel = new AgeModifierViewModel
            {
                AgeModifierList = ageModifiers,
                AgesList = agesList
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission("EditDSS")]
        public async Task<IActionResult> Edit(AgeModifierViewModel viewModel)
        {
            try
            {
                string connectionString = _connectionService.GetCurrentConnectionString();
                await using var context = new ApplicationDbContext(connectionString);

                viewModel.AgeModifierList[2].Modifier = viewModel.AgeModifierList[1].Modifier;
                context.AgeModifier.UpdateRange(viewModel.AgeModifierList);

                await context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Successfully added edited Age Modifier";
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                LoggingService.LogInformation($"UserID: {userIdClaim.Value}. Age Modifier edited successfully.");

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
