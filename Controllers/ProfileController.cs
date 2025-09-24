using LittleArkFoundation.Areas.Admin.Data;
using LittleArkFoundation.Areas.Admin.Models;
using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(ConnectionService connectionService, IWebHostEnvironment webHostEnvironment)
        {
            _connectionService = connectionService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(int id)
        {
            try
            {
                if (int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) != id)
                {
                    return Forbid();
                }

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
                LoggingService.LogError("Error: " + ex);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
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

                LoggingService.LogInformation($"UserID: {userIdClaim.Value}. Profile edit attempt");

                await using (var context = new ApplicationDbContext(connectionString))
                {
                    if (isEditPasswordEnabled)
                    {
                        byte[] passwordSalt = PasswordService.GenerateSalt();
                        string hashedPassword = PasswordService.HashPassword(user.NewUser.PasswordHash, passwordSalt);
                        user.NewUser.PasswordHash = hashedPassword;
                        user.NewUser.PasswordSalt = Convert.ToBase64String(passwordSalt);
                    }

                    if (user.NewUser.ProfilePictureFile is { Length: > 0 })
                    {
                        using var ms = new MemoryStream();
                        await user.NewUser.ProfilePictureFile.CopyToAsync(ms);
                        user.NewUser.ProfilePicture = ms.ToArray();
                        user.NewUser.ProfilePictureContentType = user.NewUser.ProfilePictureFile.ContentType;
                    }

                    context.Users.Update(user.NewUser);

                    var opdList = await context.OPD.Where(i => i.UserID == user.NewUser.UserID).ToListAsync();
                    var generalAdmissionList = await context.GeneralAdmission.Where(i => i.UserID == user.NewUser.UserID).ToListAsync();
                    var dischargeList = await context.Discharges.Where(i => i.UserID == user.NewUser.UserID).ToListAsync();

                    if (opdList.Any())
                    {
                        foreach (var item in opdList.Where(i => i.MSW != user.NewUser.Username))
                        {
                            item.MSW = user.NewUser.Username;
                        }
                    }

                    if (generalAdmissionList.Any())
                    {
                        foreach (var item in generalAdmissionList.Where(i => i.MSW != user.NewUser.Username))
                        {
                            item.MSW = user.NewUser.Username;
                        }
                    }

                    if (dischargeList.Any())
                    {
                        foreach (var item in dischargeList.Where(i => i.MSW != user.NewUser.Username))
                        {
                            item.MSW = user.NewUser.Username;
                        }
                    }

                    await context.SaveChangesAsync();

                    // Update username claim
                    if (userIdClaim.Value == user.NewUser.UserID.ToString())
                    {
                        if (User.FindFirstValue(ClaimTypes.Name) != user.NewUser.Username)
                        {
                            var identity = (ClaimsIdentity)User.Identity;

                            var oldNameClaim = identity.FindFirst(ClaimTypes.Name);
                            if (oldNameClaim != null)
                            {
                                identity.RemoveClaim(oldNameClaim);
                            }

                            identity.AddClaim(new Claim(ClaimTypes.Name, user.NewUser.Username));

                            await HttpContext.SignInAsync(
                                CookieAuthenticationDefaults.AuthenticationScheme,
                                new ClaimsPrincipal(identity)
                            );
                        }
                    }

                    LoggingService.LogInformation($"UserID: {userIdClaim.Value}. Profile edit sucessful. Edited UserID: {user.NewUser.UserID}");
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Index", user.NewUser.UserID);
            }

            return RedirectToAction("Index", "Dashboard", new { area = $"Admin" });
        }

        [HttpGet]
        public async Task<IActionResult> GetProfilePicture(int id)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using var context = new ApplicationDbContext(connectionString);

            var user = await context.Users.FirstOrDefaultAsync(u => u.UserID == id);
            
            if (user == null || user.ProfilePicture == null || user.ProfilePicture.Length == 0)
            {
                var defaultPath = Path.Combine(_webHostEnvironment.WebRootPath, "resources", "profile-icon-design-free-vector.jpg");
                return PhysicalFile(defaultPath, "image/jpeg");
            }

            return File(user.ProfilePicture, user.ProfilePictureContentType); // or detect actual content type
        }
    }
}
