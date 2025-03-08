using LittleArkFoundation.Areas.Admin.Data;
using LittleArkFoundation.Areas.Admin.Models;
using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using LittleArkFoundation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security;
using System.Security.Claims;

namespace LittleArkFoundation.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin")]
    [HasPermission("ManageRoles")]
    public class RolesController : Controller
    {
        private readonly ConnectionService _connectionService;

        public RolesController(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                string connectionString = _connectionService.GetCurrentConnectionString();

                await using (var context = new ApplicationDbContext(connectionString))
                {
                    var role = await context.Roles.ToListAsync();

                    var viewModel = new RolesViewModel()
                    {
                        Roles = role
                    };

                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex.Message);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // TODO: Implement search role

        public async Task<IActionResult> Create(string name)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                LoggingService.LogInformation($"Role creation attempt. Role: {name}, UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");

                string connectionString = _connectionService.GetCurrentConnectionString();

                await using (var context = new ApplicationDbContext(connectionString))
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        var role = new RolesModel()
                        {
                            RoleID = await new RolesRepository(connectionString).GenerateRoleIDAsync(),
                            RoleName = name
                        };

                        await context.Roles.AddAsync(role);
                        await context.SaveChangesAsync();

                        TempData["SuccessMessage"] = $"Successfully added new role! RoleName: {name}";
                    }
                }

                LoggingService.LogInformation($"Role creation attempt successful. Role: {name}, UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex.Message);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                string connectionString = _connectionService.GetCurrentConnectionString();

                await using (var context = new ApplicationDbContext(connectionString))
                {
                    var role = await context.Roles
                        .Include(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permissions)
                        .FirstOrDefaultAsync(r => r.RoleID == id);

                    if (role == null)
                        return NotFound();

                    var allPermissions = await context.Permissions.ToListAsync();

                    var viewModel = new RolesViewModel
                    {
                        NewRole = role,
                        Permissions = allPermissions.Select(p => new PermissionCheckbox
                        {
                            PermissionID = p.PermissionID,
                            Name = p.Name,
                            IsSelected = role.RolePermissions.Any(rp => rp.PermissionID == p.PermissionID)
                        }).ToList()
                    };

                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex.Message);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RolesViewModel roleViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(roleViewModel);
            }

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                LoggingService.LogInformation($"Role edit attempt. Role: {roleViewModel.NewRole.RoleName}, UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");

                string connectionString = _connectionService.GetCurrentConnectionString();

                await using (var context = new ApplicationDbContext(connectionString))
                {
                    var role = await context.Roles
                        .Include(r => r.RolePermissions)
                        .FirstOrDefaultAsync(r => r.RoleID == roleViewModel.NewRole.RoleID);

                    if (role == null)
                        return NotFound();

                    // Update Role Name
                    role.RoleName = roleViewModel.NewRole.RoleName;

                    // Remove existing permissions
                    context.RolePermissions.RemoveRange(role.RolePermissions);

                    // Add selected permissions
                    var selectedPermissions = roleViewModel.Permissions
                        .Where(p => p.IsSelected)
                        .Select(p => new RolePermissionsModel
                        {
                            RoleID = role.RoleID,
                            PermissionID = p.PermissionID
                        });

                    await context.RolePermissions.AddRangeAsync(selectedPermissions);
                    await context.SaveChangesAsync();
                }

                LoggingService.LogInformation($"Role edit attempt successful. Role: {roleViewModel.NewRole.RoleName}, UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex.Message);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                LoggingService.LogInformation($"Role delete attempt. RoleID: {id}, UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");

                string connectionString = _connectionService.GetCurrentConnectionString();
                string roleName = await new RolesRepository(connectionString).GetRoleNameByRoleID(id);

                await using (var context = new ApplicationDbContext(connectionString))
                {
                    if (id == 0) return NotFound();

                    if (id == 1 || id == 2 || id == 3)
                    {
                        TempData["ErrorMessage"] = $"Not allowed to delete {roleName}";
                        return RedirectToAction("Index");
                    }

                    var role = await context.Roles.FindAsync(id);

                    context.Roles.Remove(role);
                    await context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"Successfully deleted Role: {roleName}";
                LoggingService.LogInformation($"Role delete attempt successful. RoleID: {id}, UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");
                return RedirectToAction("Index");
            }
            catch (SqlException ex)
            {
                LoggingService.LogError("SQL Error: " + ex.Message);
                TempData["ErrorMessage"] = "SQL Error: " + ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex.Message);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

    }
}
