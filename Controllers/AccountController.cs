using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using LittleArkFoundation.Data;
using Microsoft.Data.SqlClient;
using System.Data;
using LittleArkFoundation.Areas.Admin.Data;

namespace LittleArkFoundation.Controllers
{
    public class AccountController : Controller
    {
        private readonly ConnectionService _connectionService;
        private readonly DatabaseService _databaseService;

        public AccountController(ConnectionService connectionService, DatabaseService databaseService)
        {
            _connectionService = connectionService;
            _databaseService = databaseService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(int userID, string password)
        {
            string connectionString = _connectionService.GetDefaultConnectionString();

            try
            {
                LoggingService.LogInformation($"Login attempt. UserID: {userID}, DateTime: {DateTime.Now}");

                var user = await new UsersRepository(connectionString).GetUserAsync(userID, password);

                if (user == null)
                {
                    LoggingService.LogInformation($"Invalid login attempt. UserID: {userID}, DateTime: {DateTime.Now}");
                    TempData["LoginError"] = "Invalid User ID or Password. Please try again.";
                    return RedirectToAction("Index", "Home");
                }

                string role = await new RolesRepository(connectionString).GetRoleNameByRoleID(user.RoleID);
                var permissions = await new PermissionsRepository(connectionString).GetPermissionsByRoleID(user.RoleID);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, role)
                };

                // Add permissions as claims
                foreach (var permission in permissions)
                {
                    claims.Add(new Claim("Permission", permission)); // Custom claim for permissions
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                HttpContext.Session.SetString("ConnectionString", connectionString);
                HttpContext.Session.SetString("DatabaseName", _databaseService.GetSelectedDatabaseInConnectionString(connectionString));

                LoggingService.LogInformation($"User logged in. UserID: {userID}, Role: {role}, DateTime: {DateTime.Now}");

                // Redirect based on role
                if (role == "Donor")
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Donor" });
                }
                else if (role == "Doctor")
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Doctor" });
                }
               
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            catch (SqlException ex)
            {
                LoggingService.LogError("SQL Error: " + ex.Message);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error: " + ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Logout()
        {
            //await HttpContext.SignOutAsync("CookieAuth");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("ConnectionString");
            HttpContext.Session.Remove("DatabaseName");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
            {
                LoggingService.LogInformation($"User logged out. UserID: {userIdClaim.Value}, DateTime: {DateTime.Now}");
            }
            else
            {
                LoggingService.LogWarning("User logged out but no UserID claim found.");
            }
            return RedirectToAction("Index", "Home");
        }

        
        private static Dictionary<int, DateTime> lastRequestTimes = new Dictionary<int, DateTime>();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCode(int userID)
        {
            if (userID == 0)
            {
                return Json(new { success = false, message = "User ID is required!" });
            }

            if (lastRequestTimes.TryGetValue(userID, out DateTime lastRequest))
            {
                TimeSpan timeSinceLastRequest = DateTime.UtcNow - lastRequest;

                if (timeSinceLastRequest.TotalSeconds < 30)
                {
                    return Json(new { success = false, message = $"Please wait {30 - timeSinceLastRequest.TotalSeconds:F0} seconds before requesting again." });
                }
            }

            string verificationCode = EmailService.GetSecureNumericVerificationCode(6);
            lastRequestTimes[userID] = DateTime.UtcNow;

            // Store session values correctly
            HttpContext.Session.SetString("VerificationCode", verificationCode);
            HttpContext.Session.SetString("CodeExpiresAt", DateTime.UtcNow.AddMinutes(3).Ticks.ToString());

            // Send email asynchronously
            string connectionString = _connectionService.GetDefaultConnectionString();
            using (var context = new ApplicationDbContext(connectionString))
            {
                var user = await context.Users.FindAsync(userID);
                if (user == null || string.IsNullOrEmpty(user.Email))
                {
                    return Json(new { success = false, message = "User not found or email missing!" });
                }

                EmailService _emailService = new EmailService();
                string emailMessage = $"Your verification code is: {verificationCode}";

                bool emailSent = await _emailService.SendEmailAsync(user.Email, "Verification Code", emailMessage);
                if (!emailSent)
                {
                    return Json(new { success = false, message = "Failed to send email. Try again later." });
                }
            }

            return Json(new { success = true, message = "Verification code sent successfully!" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCode(int userID, string code)
        {
            if (userID == 0 || string.IsNullOrEmpty(code))
            {
                return Json(new { success = false, message = "User ID and Code are required!" });
            }

            string storedCode = HttpContext.Session.GetString("VerificationCode");
            string expiresAtString = HttpContext.Session.GetString("CodeExpiresAt");

            if (string.IsNullOrEmpty(storedCode) || string.IsNullOrEmpty(expiresAtString))
            {
                return Json(new { success = false, message = "No verification code found. Please request a new one." });
            }

            DateTime expiresAt = new DateTime(long.Parse(expiresAtString), DateTimeKind.Utc);
            if (DateTime.UtcNow > expiresAt)
            {
                return Json(new { success = false, message = "Verification code expired. Please request a new one." });
            }

            if (code != storedCode)
            {
                return Json(new { success = false, message = "Invalid code. Please try again." });
            }

            // Store userID in session after successful verification
            HttpContext.Session.SetInt32("VerifiedUserID", userID);

            // Clear session after success
            HttpContext.Session.Remove("VerificationCode");
            HttpContext.Session.Remove("CodeExpiresAt");

            return Json(new { success = true, message = "Code verified successfully!" });
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            // Check if user is verified
            int? verifiedUserID = HttpContext.Session.GetInt32("VerifiedUserID");
            if (verifiedUserID == null)
            {
                // Redirect to verification page if not verified
                return RedirectToAction("Index", "Home");
            }

            ViewBag.UserID = verifiedUserID; // Pass userID to the view
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int userID, string newPassword)
        {
            try
            {
                LoggingService.LogInformation($"User reset password attempt. UserID: {userID}, DateTime: {DateTime.Now}");

                string connectionString = _connectionService.GetDefaultConnectionString();

                using (var context = new ApplicationDbContext(connectionString))
                {
                    var user = await context.Users.FindAsync(userID);

                    byte[] passwordSalt = PasswordService.GenerateSalt();
                    string hashedPassword = PasswordService.HashPassword(newPassword, passwordSalt);

                    user.PasswordHash = hashedPassword;
                    user.PasswordSalt = Convert.ToBase64String(passwordSalt);

                    context.Users.Update(user);
                    await context.SaveChangesAsync();
                }

                HttpContext.Session.Remove("VerifiedUserID");
                TempData["ResetPasswordSuccess"] = "Your password has been reset successfully.";
                LoggingService.LogInformation($"User reset password successful. UserID: {userID}, DateTime: {DateTime.Now}");
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Password reset error: {ex.Message}");
                LoggingService.LogError("Error: " + ex.Message);
                return RedirectToAction("ResetPassword");
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            LoggingService.LogWarning($"Access denied. UserID: {User.FindFirst(ClaimTypes.NameIdentifier)?.Value}, DateTime: {DateTime.Now}");
            return View();
        }
    }
}