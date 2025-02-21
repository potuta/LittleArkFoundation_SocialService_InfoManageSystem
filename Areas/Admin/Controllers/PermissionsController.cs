using LittleArkFoundation.Areas.Admin.Models;
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
    [HasPermission("ManagePermissions")]
    public class PermissionsController : Controller
    {
        private readonly ConnectionService _connectionService;

        public PermissionsController(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<IActionResult> Index(string dbType)
        {
            string connectionString = _connectionService.GetConnectionString(dbType);

            await using (var context = new ApplicationDbContext(connectionString))
            {
                var permissions = await context.Permissions.ToListAsync();

                var viewModel = new PermissionsViewModel()
                {
                    Permissions = permissions
                };

                return View(viewModel);
            }
        }

        // TODO: Implement search permissions

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string dbType, string name)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                LoggingService.LogInformation($"Permission creation attempt. UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");

                string connectionString = _connectionService.GetConnectionString(dbType);

                await using (var context = new ApplicationDbContext(connectionString))
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        var permission = new PermissionsModel()
                        {
                            Name = name
                        };

                        await context.Permissions.AddAsync(permission);
                        await context.SaveChangesAsync();

                        TempData["CreateSuccess"] = $"Successfully added new permission! PermissionName: {permission.Name}";
                        LoggingService.LogInformation($"Permission creation successful. Added PermissionName: {permission.Name}. Added by UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex.Message);
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        // TODO: Implement permissions edit
        public async Task<IActionResult> Edit()
        {
            return View();
        }

        public async Task<IActionResult> Delete(string dbType, int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                LoggingService.LogInformation($"Permission deletion attempt. UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");

                string connectionString = _connectionService.GetConnectionString(dbType);

                await using (var context = new ApplicationDbContext(connectionString))
                {
                    if (id != 0)
                    {
                        var permission = await context.Permissions.FindAsync(id);

                        context.Permissions.Remove(permission);
                        await context.SaveChangesAsync();

                        TempData["CreateSuccess"] = $"Successfully deleted permission! PermissionID: {id}";
                        LoggingService.LogInformation($"Permission deletion successful. Deleted PermissionID: {id}. Deleted by UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex.Message);
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }


    }
}
