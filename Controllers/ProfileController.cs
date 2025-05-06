using LittleArkFoundation.Areas.Admin.Data;
using LittleArkFoundation.Areas.Admin.Models;
using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Security.Claims;

namespace LittleArkFoundation.Controllers
{
    [HasPermission("ManageProfile")]
    public class ProfileController : Controller
    {
        private readonly ConnectionService _connectionService;

        public ProfileController(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<IActionResult> Index(int id)
        {
            try
            {
                string connectionString = _connectionService.GetCurrentConnectionString();
                await using var context = new ApplicationDbContext(connectionString);
                var user = await context.Users.FindAsync(id);

                var viewModel = new UsersViewModel
                {
                    NewUser = user,
                    Roles = await new RolesRepository(_connectionService).GetRolesAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex.Message);
                return RedirectToAction("Index", id);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UsersViewModel user, bool isEditPasswordEnabled)
        {
            Console.WriteLine($"viewModel is null: {user == null}");

            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine("ModelState Error: " + error.ErrorMessage, error.Exception);
                    }
                }
                user.Roles = await new RolesRepository(_connectionService).GetRolesAsync();
                return RedirectToAction("Index", user.NewUser.UserID);
            }

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                string connectionString = _connectionService.GetCurrentConnectionString();

                LoggingService.LogInformation($"Profile edit attempt. UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");

                await using (var context = new ApplicationDbContext(connectionString))
                {
                    if (isEditPasswordEnabled)
                    {
                        byte[] passwordSalt = PasswordService.GenerateSalt();
                        string hashedPassword = PasswordService.HashPassword(user.NewUser.PasswordHash, passwordSalt);
                        user.NewUser.PasswordHash = hashedPassword;
                        user.NewUser.PasswordSalt = Convert.ToBase64String(passwordSalt);
                    }

                    context.Users.Update(user.NewUser);
                    await context.SaveChangesAsync();

                    LoggingService.LogInformation($"Profile edit sucessful. Edited UserID: {user.NewUser.UserID}. Edited by UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex.Message);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Index", user.NewUser.UserID);
            }

            return RedirectToAction("Index", "Dashboard", new { area = $"Admin" });
        }
    }
}
