using LittleArkFoundation.Areas.Admin.Models.Criteria;
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

            var viewModel = new CriteriaViewModel()
            {
                CriteriaList = criteriaList,
                CriteriaDisplayNamesList = criteriaDisplayNamesList,
                Criteria = new CriteriaModel()
            };

            return View(viewModel);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission("CreateDSS")]
        public async Task<IActionResult> Create(CriteriaViewModel viewModel)
        {
            try
            {
                string connectionString = _connectionService.GetCurrentConnectionString();
                await using var context = new ApplicationDbContext(connectionString);

                var diagnosis = viewModel.Criteria.Diagnosis.Trim().ToUpper();

                bool exists = await context.Criteria.AnyAsync(c => c.Diagnosis.ToUpper() == diagnosis);

                if (exists)
                {
                    TempData["ErrorMessage"] = "A criteria with this name already exists.";
                    //ModelState.AddModelError("Criteria.Diagnosis", "A criteria with this name already exists.");
                    return RedirectToAction("Index");
                }

                int maxDiagnosisId = await context.Criteria
                    .MaxAsync(c => (int?)c.DiagnosisID) ?? 0;

                int diagnosisId = maxDiagnosisId + 1;

                var newCriteria = new CriteriaModel
                {
                    Diagnosis = diagnosis,
                    Weight = viewModel.Criteria.Weight,
                    IsDisplayName = true,
                    DiagnosisID = diagnosisId
                };

                context.Criteria.Add(newCriteria);
                await context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Successfully added new Diagnosis: {diagnosis}";
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                LoggingService.LogInformation($"UserID: {userIdClaim.Value}. Criteria Diagnosis added successfully. Criteria Diagnosis: {diagnosis}");
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

        [HasPermission("EditDSS")]
        public async Task<IActionResult> Edit(int id)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var criteria = await context.Criteria.FindAsync(id);

            if (criteria == null)
            {
                return NotFound();
            }

            var criteriaList = await context.Criteria
                .Where(c => c.DiagnosisID == criteria.DiagnosisID)
                .OrderByDescending(c => c.IsDisplayName) // Display name first
                .ToListAsync();

            var viewModel = new CriteriaViewModel()
            {
                Criteria = criteria,
                CriteriaList = criteriaList
            };

            return View(viewModel);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission("EditDSS")]
        public async Task<IActionResult> Edit(CriteriaViewModel viewModel)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var criteriaList = viewModel.CriteriaList;
                var criteria = await context.Criteria.FindAsync(criteriaList[0].Id);

                if (criteria == null)
                {
                    TempData["ErrorMessage"] = "Criteria not found.";
                    return RedirectToAction("Index");
                }

                int weight = criteriaList.Find(c => c.IsDisplayName)?.Weight ?? 0;

                foreach (var item in criteriaList)
                {
                    var existingCriteria = await context.Criteria.FindAsync(item.Id);
                    var diagnosis = item.Diagnosis.Trim().ToUpper();

                    bool exists = await context.Criteria.AnyAsync(c => c.Diagnosis.ToUpper() == diagnosis && c.DiagnosisID != criteria.DiagnosisID);

                    if (exists)
                    {
                        TempData["ErrorMessage"] = $"A criteria with the name '{diagnosis}' already exists.";
                        await transaction.RollbackAsync();
                        return View(viewModel);
                    }

                    if (existingCriteria != null)
                    {

                        existingCriteria.Diagnosis = diagnosis;
                        existingCriteria.Weight = weight;

                        context.Criteria.Update(existingCriteria);
                    }
                    else
                    {
                        var newCriteria = new CriteriaModel
                        {
                            Diagnosis = diagnosis,
                            Weight = weight,
                            IsDisplayName = false,  
                            DiagnosisID = criteria.DiagnosisID
                        };

                        context.Criteria.Add(newCriteria);
                    }
                }

                var existingCriteriaList = await context.Criteria
                    .Where(c => c.DiagnosisID == criteria.DiagnosisID)
                    .ToListAsync(); // now all rows are in memory

                var existingCriteriaToDelete = existingCriteriaList
                    .Where(c => !criteriaList.Any(cl => cl.Id == c.Id) && !c.IsDisplayName)
                    .ToList();

                context.Criteria.RemoveRange(existingCriteriaToDelete);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] =
                    $"Successfully edited Diagnosis: {criteria.Diagnosis}";

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                LoggingService.LogInformation($"UserID: {userIdClaim.Value}. Criteria Diagnosis edited successfully. Criteria Diagnosis: {criteria.Diagnosis}");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "An error occurred while saving changes.";
                LoggingService.LogError(ex.ToString());
                return RedirectToAction("Index");
            }
        }

        [HasPermission("CreateDSS")]
        public async Task<IActionResult> Delete(int id)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var criteria = await context.Criteria.FindAsync(id);

                if (criteria == null)
                {
                    TempData["ErrorMessage"] = "Criteria not found.";
                    return RedirectToAction("Index");
                }

                var criteriaList = await context.Criteria
                    .Where(c => c.DiagnosisID == criteria.DiagnosisID)
                    .ToListAsync();

                //if (!criteriaList.Any())
                //{
                //    TempData["ErrorMessage"] = "No criteria found to delete.";
                //    return RedirectToAction("Index");
                //}

                context.Criteria.RemoveRange(criteriaList);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = $"Successfully deleted Diagnosis: {criteria.Diagnosis}";

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                LoggingService.LogInformation($"UserID: {userIdClaim.Value}. Criteria Diagnosis deleted successfully. Criteria Diagnosis: {criteria.Diagnosis}");

                return RedirectToAction("Index");
            }
            catch (SqlException ex)
            {
                await transaction.RollbackAsync();
                LoggingService.LogError($"SQL Error: {ex}");
                TempData["ErrorMessage"] = "SQL Error: " + ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                LoggingService.LogError($"Error: {ex}");
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

    }
}
