using LittleArkFoundation.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LittleArkFoundation.Areas.Admin.Models.Database
{
    public class DatabaseViewModel
    {
        public string? DatabaseName { get; set; }
        public string? CurrentConnectionString { get; set; }
        public string? DefaultConnectionString { get; set; }
        public Dictionary<string, string>? Databases { get; set; }
        public List<string>? DatabaseBackupFiles { get; set; }
    }
}
