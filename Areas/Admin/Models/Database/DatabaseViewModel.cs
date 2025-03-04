using LittleArkFoundation.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LittleArkFoundation.Areas.Admin.Models.Database
{
    public class DatabaseViewModel
    {
        public Dictionary<string, string>? Databases { get; set; }
    }
}
