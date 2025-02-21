using LittleArkFoundation.Areas.Admin.Data;
using LittleArkFoundation.Areas.Admin.Models;
using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security;

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

        public async Task<IActionResult> Index(string dbType)
        {
            string connectionString = _connectionService.GetConnectionString(dbType);

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

        // TODO: Implement search role

        // TODO: add logs for creating roles
        public async Task<IActionResult> Create(string dbType, string name)
        {
            string connectionString = _connectionService.GetConnectionString(dbType);

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

                    TempData["CreateSuccess"] = $"Successfully added new role! RoleName: {name}";
                }
            }

            return RedirectToAction("Index");
        }

        // TODO: add logs for editing roles
        public async Task<IActionResult> Edit(string dbType, int id)
        {
            string connectionString = _connectionService.GetConnectionString(dbType);

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string dbType, RolesViewModel roleViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(roleViewModel);
            }

            string connectionString = _connectionService.GetConnectionString(dbType);

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

            return RedirectToAction("Index");
        }

        // TODO: add logs to delete role

        public async Task<IActionResult> Delete(string dbType, int id)
        {
            string connectionString = _connectionService.GetConnectionString(dbType);
            string roleName = await new RolesRepository(connectionString).GetRoleNameByRoleID(id);

            await using (var context = new ApplicationDbContext(connectionString))
            {
                if (id == 0) return NotFound();

                if (id == 1 || id == 2 || id == 3)
                {
                    TempData["DeleteError"] = $"Not allowed to delete {roleName}";
                    return RedirectToAction("Index");
                }

                var role = await context.Roles.FindAsync(id);

                context.Roles.Remove(role);
                await context.SaveChangesAsync();
            }

            TempData["DeleteSuccess"] = $"Successfully deleted Role: {roleName}";
            return RedirectToAction("Index");
        }

    }
}
