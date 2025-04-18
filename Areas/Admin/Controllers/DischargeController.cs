using LittleArkFoundation.Authorize;
using Microsoft.AspNetCore.Mvc;

namespace LittleArkFoundation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [HasPermission("ManageDischarge")]
    public class DischargeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
