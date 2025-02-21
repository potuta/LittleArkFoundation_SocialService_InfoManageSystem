namespace LittleArkFoundation.Areas.Admin.Models
{
    public class PermissionsViewModel 
    {
        public IEnumerable<PermissionsModel>? Permissions { get; set; }
        public PermissionsModel? NewPermission { get; set; }
    }
}
