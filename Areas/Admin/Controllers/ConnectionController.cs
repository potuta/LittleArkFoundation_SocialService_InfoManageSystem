using LittleArkFoundation.Authorize;
using LittleArkFoundation.Data;
using Microsoft.AspNetCore.Mvc;

namespace LittleArkFoundation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [HasPermission("ManageDatabase")]
    [Route("Admin/[controller]/[action]")]
    [ApiController]
    public class ConnectionController : ControllerBase
    {
        private readonly ConnectionService _connectionService;
        private readonly DatabaseService _databaseService;

        public ConnectionController(ConnectionService connectionService, DatabaseService databaseService)
        {
            _connectionService = connectionService;
            _databaseService = databaseService;
        }

        [HttpGet]
        public IActionResult IsConnectionArchived()
        {
            var currentConnectionString = _connectionService.GetCurrentConnectionString();
            var defaultConnectionString = _connectionService.GetDefaultConnectionString();

            bool isArchived = currentConnectionString != defaultConnectionString;

            var currentDatabaseName = _databaseService.GetSelectedDatabaseInConnectionString(currentConnectionString);
            var defaultDatabaseName = _databaseService.GetSelectedDatabaseInConnectionString(defaultConnectionString);

            bool isTemp = currentDatabaseName == $"{defaultDatabaseName}_Temp";
            bool isDefault = currentDatabaseName == defaultDatabaseName;

            return Ok(new {isArchived = isArchived, isTemp = isTemp, isDefault = isDefault});
        }
    }
}
