using LittleArkFoundation.Authorize;
using Microsoft.AspNetCore.Mvc;

namespace LittleArkFoundation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [HasPermission("ManageOPD")]
    public class OPDController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
