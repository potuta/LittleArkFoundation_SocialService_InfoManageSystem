﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LittleArkFoundation.Data;
using System.Security.Claims;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using LittleArkFoundation.Areas.Admin.Data;
using LittleArkFoundation.Areas.Admin.Models;
using LittleArkFoundation.Authorize;
using Microsoft.IdentityModel.Tokens;

namespace LittleArkFoundation.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin")]
    [HasPermission("ManageUsers")]
    public class UsersController : Controller
    {
        private readonly ConnectionService _connectionService;

        public UsersController(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<IActionResult> Index(bool isArchive)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using (var context = new ApplicationDbContext(connectionString))
            {

                if (isArchive)
                {
                    var usersArchives = await context.UsersArchives.ToListAsync();

                    var viewArchivesModel = new UsersViewModel
                    {
                        UsersArchives = usersArchives,
                        //NewUserArchive = new UsersArchivesModel(),
                        Roles = await new RolesRepository(_connectionService).GetRolesAsync()
                    };

                    ViewBag.isArchive = isArchive;
                    return View(viewArchivesModel);
                }

                var users = await context.Users.ToListAsync();

                var viewModel = new UsersViewModel
                {
                    Users = users,
                    NewUser = new UsersModel(),
                    Roles = await new RolesRepository(_connectionService).GetRolesAsync()
                };

                ViewBag.isArchive = isArchive;
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Search(string searchString, bool isArchive)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using (var context = new ApplicationDbContext(connectionString))
            {
                if (!string.IsNullOrEmpty(searchString))
                {
                    var loweredSearch = searchString.ToLower();
                    if (isArchive)
                    {
                        var usersArchive = await context.UsersArchives
                                            .Where(u => string.IsNullOrEmpty(searchString) ||
                                            u.Username.ToLower().Contains(loweredSearch) ||
                                            u.UserID.ToString().Contains(searchString)) 
                                            .ToListAsync();


                        var viewArchivesModel = new UsersViewModel
                        {
                            UsersArchives = usersArchive,
                            Roles = await new RolesRepository(_connectionService).GetRolesAsync()
                        };

                        ViewBag.isArchive = isArchive;
                        return View("Index", viewArchivesModel);
                    }

                    var users = await context.Users
                                    .Where(u => string.IsNullOrEmpty(searchString) ||
                                    u.Username.ToLower().Contains(loweredSearch) ||
                                    u.UserID.ToString().Contains(searchString)) 
                                    .ToListAsync();

                    var viewModel = new UsersViewModel
                    {
                        Users = users,
                        NewUser = new UsersModel(),
                        Roles = await new RolesRepository(_connectionService).GetRolesAsync()
                    };

                    ViewBag.isArchive = isArchive;
                    return View("Index", viewModel);
                }

                return RedirectToAction("Index", new {isArchive});
            }
        }

        public async Task<IActionResult> SortBy(string sortByRoleID, bool isArchive)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using (var context = new ApplicationDbContext(connectionString))
            {
                if (!string.IsNullOrEmpty(sortByRoleID))
                {

                    if (isArchive)
                    {
                        var usersArchive = await context.UsersArchives
                            .Where(u => string.IsNullOrEmpty(sortByRoleID) ||
                            u.RoleID.ToString().Contains(sortByRoleID))
                            .ToListAsync();

                        var viewArchivesModel = new UsersViewModel
                        {
                            UsersArchives = usersArchive,
                            Roles = await new RolesRepository(_connectionService).GetRolesAsync()
                        };

                        ViewBag.sortBy = await new RolesRepository(_connectionService).GetRoleNameByRoleID(int.Parse(sortByRoleID));
                        ViewBag.isArchive = isArchive;
                        return View("Index", viewArchivesModel);
                    }

                    var users = await context.Users
                        .Where (u => string.IsNullOrEmpty(sortByRoleID) ||
                        u.RoleID.ToString().Contains(sortByRoleID))
                        .ToListAsync();

                    var viewModel = new UsersViewModel
                    {
                        Users = users,
                        Roles = await new RolesRepository(_connectionService).GetRolesAsync()
                    };

                    ViewBag.sortBy = await new RolesRepository(_connectionService).GetRoleNameByRoleID(int.Parse(sortByRoleID));
                    ViewBag.isArchive = isArchive;
                    return View("Index", viewModel);

                }

                return RedirectToAction("Index", new { isArchive });
            }
        }

        // CREATE: Create a new user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsersViewModel viewModel)
        {
            //Console.WriteLine($"viewModel is null: {viewModel == null}");

            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine("ModelState Error: " + error.ErrorMessage);
                    }
                }
                return View("Index", viewModel);
            }

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                LoggingService.LogInformation($"User creation attempt. UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");

                string connectionString = _connectionService.GetCurrentConnectionString();

                await using (var context = new ApplicationDbContext(connectionString))
                {
                    int newUserID = await new UsersRepository(connectionString).GenerateUserIDAsync(viewModel.NewUser.RoleID);
                    viewModel.NewUser.UserID = newUserID;

                    byte[] passwordSalt = PasswordService.GenerateSalt();
                    string hashedPassword = PasswordService.HashPassword(viewModel.NewUser.PasswordHash, passwordSalt);

                    viewModel.NewUser.PasswordHash = hashedPassword;
                    viewModel.NewUser.PasswordSalt = Convert.ToBase64String(passwordSalt);
                    viewModel.NewUser.CreatedAt = DateTime.Now;

                    await context.Users.AddAsync(viewModel.NewUser);
                    await context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Successfully added new user! UserID: {viewModel.NewUser.UserID} Username: {viewModel.NewUser.Username}";
                    LoggingService.LogInformation($"User creation successful. Created new user UserID: {viewModel.NewUser.UserID}. Created by UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");
                }
            }
            catch (SqlException ex)
            {
                LoggingService.LogError("SQL Error: " + ex);
                TempData["ErrorMessage"] = "SQL Error: " + ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        // 🟢 READ: Show details
        public async Task<IActionResult> Details(int id)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();

            await using (var context = new ApplicationDbContext(connectionString))
            {
                var user = await context.Users.FindAsync(id);
                if (user == null) return NotFound();
                return View(user);
            }
        }

        // 🟡 EDIT: Show edit page
        public async Task<IActionResult> Edit(int id)
        {
            string connectionString = _connectionService.GetCurrentConnectionString();
            await using (var context = new ApplicationDbContext(connectionString))
            {
                var user = await context.Users.FindAsync(id);
                if (user == null) return NotFound();

                var viewModel = new UsersViewModel
                {
                    Users = new List<UsersModel>(),
                    NewUser = user,
                    Roles = await new RolesRepository(_connectionService).GetRolesAsync()
                };

                return View(viewModel);
            }
        }

        // 🔵 UPDATE: Save changes
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
                return View("Index", user);
            }

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                string connectionString = _connectionService.GetCurrentConnectionString();

                LoggingService.LogInformation($"User edit attempt. UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");

                await using (var context = new ApplicationDbContext(connectionString))
                {
                    //context.Entry(user).State = EntityState.Modified;
                    //context.Update(user.NewUser);

                    if (isEditPasswordEnabled)
                    {
                        byte[] passwordSalt = PasswordService.GenerateSalt();
                        string hashedPassword = PasswordService.HashPassword(user.NewUser.PasswordHash, passwordSalt);
                        user.NewUser.PasswordHash = hashedPassword;
                        user.NewUser.PasswordSalt = Convert.ToBase64String(passwordSalt);
                    }

                    context.Users.Update(user.NewUser);
                    await context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Successfully edited user! UserID: {user.NewUser.UserID} Username: {user.NewUser.Username}";
                    LoggingService.LogInformation($"User edit sucessful. Edited UserID: {user.NewUser.UserID}. Edited by UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Index", new { isArchive = false });
            }

            return RedirectToAction("Index");
        }

        // 🟡 ARCHIVE: Archive the user
        public async Task<IActionResult> Archive(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                string connectionString = _connectionService.GetCurrentConnectionString();

                LoggingService.LogInformation($"User archive attempt. UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");
                await using (var context = new ApplicationDbContext(connectionString))
                {
                    var user = await context.Users.FindAsync(id);

                    if (user.UserID.ToString() == userIdClaim.Value)
                    {
                        TempData["ErrorMessage"] = "Can't archive the user you're currently using.";
                        return RedirectToAction("Index", new { isArchive = false });
                    }

                    var userArchive = new UsersArchivesModel()
                    {
                        UserID = user.UserID,
                        Username = user.Username,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        PasswordHash = user.PasswordHash,
                        PasswordSalt = user.PasswordSalt,
                        RoleID = user.RoleID,
                        CreatedAt = user.CreatedAt,
                        ArchivedAt = DateTime.Now,
                        ArchivedBy = $"UserID: {userIdClaim.Value}"
                    };

                    await context.UsersArchives.AddAsync(userArchive);
                    context.Users.Remove(user);

                    await context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Successfully archived user! UserID: {userArchive.UserID} Username: {userArchive.Username}";
                    LoggingService.LogInformation($"User archive successful. Archived UserID: {userArchive.UserID}. Archived by UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Index", new { isArchive = false });
            }

            return RedirectToAction("Index", new {isArchive = false});
        }

        // 🟡 UNARCHIVE: Unarchive the user
        public async Task<IActionResult> Unarchive(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                string connectionString = _connectionService.GetCurrentConnectionString();

                LoggingService.LogInformation($"User unarchive attempt. UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");

                await using (var context = new ApplicationDbContext(connectionString))
                {
                    var userArchive = await context.UsersArchives.FindAsync(id);

                    var user = new UsersModel()
                    {
                        UserID = userArchive.UserID,
                        Username = userArchive.Username,
                        Email = userArchive.Email,
                        PhoneNumber = userArchive.PhoneNumber,
                        PasswordHash = userArchive.PasswordHash,
                        PasswordSalt = userArchive.PasswordSalt,
                        RoleID = userArchive.RoleID,
                        CreatedAt = userArchive.CreatedAt,
                    };

                    await context.Users.AddAsync(user);
                    context.UsersArchives.Remove(userArchive);

                    await context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Successfully unarchived user! UserID: {user.UserID} Username: {user.Username}";
                    LoggingService.LogInformation($"User unarchive successful. Unarchived UserID: {user.UserID}. Archived by UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Index", new { isArchive = false });
            }

            return RedirectToAction("Index", new { isArchive = true });
        }

    }
}
