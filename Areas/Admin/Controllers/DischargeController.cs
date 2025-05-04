using LittleArkFoundation.Areas.Admin.Models.Discharges;
using LittleArkFoundation.Areas.Admin.Models.Patients;
using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LittleArkFoundation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [HasPermission("ManageDischarge")]
    public class DischargeController : Controller
    {
        private readonly ConnectionService _connectionService;
        public DischargeController(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DischargePatient(PatientsViewModel patientsViewModel)
        {
            try
            {
                string connectionString = _connectionService.GetCurrentConnectionString();
                await using var context = new ApplicationDbContext(connectionString);

                var discharge = patientsViewModel.Discharge;
                var patient = await context.Patients.FindAsync(discharge.PatientID);

                // Example: Map data to a discharge entity
                var dischargeEntity = new DischargesModel
                {
                    PatientID = discharge.PatientID,
                    AssessmentID = discharge.AssessmentID,
                    FirstName = discharge.FirstName,
                    MiddleName = discharge.MiddleName,
                    LastName = discharge.LastName,
                    Ward = discharge.Ward,
                    Class = discharge.Class,
                    ProcessedDate = discharge.ProcessedDate,
                    DischargedDate = discharge.DischargedDate,
                    ReceivedHB = discharge.ReceivedHB,
                    IssuedMSS = discharge.IssuedMSS,
                    Duration = discharge.Duration,
                    PHICCategory = discharge.PHICCategory,
                    PHICUsed = discharge.PHICUsed,
                    MSW = discharge.MSW,
                    RemarksIfNo = discharge.RemarksIfNo,
                    UserID = discharge.UserID,
                };

                patient.IsActive = false; // Mark patient as inactive

                // Save to database — replace with your actual DB context/service
                context.Discharges.Add(dischargeEntity);
                context.Patients.Update(patient);
                await context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Patient successfully discharged.";
                return RedirectToAction("Index", "Form");
            }
            catch (SqlException se)
            {
                TempData["ErrorMessage"] = "An error occurred while processing your request.";
                return RedirectToAction("Index", "Form");
            }
            catch (Exception ex) 
            {
                TempData["ErrorMessage"] = "An error occurred while processing your request.";
                return RedirectToAction("Index", "Form");
            }
        }

    }
}
