using System.Diagnostics;
using LittleArkFoundation.Data;
using LittleArkFoundation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LittleArkFoundation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ConnectionService _connectionService;

        public HomeController(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
