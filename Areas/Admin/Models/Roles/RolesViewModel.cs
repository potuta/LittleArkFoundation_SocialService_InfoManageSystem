namespace LittleArkFoundation.Areas.Admin.Models
{
    public class RolesViewModel
    {
        public IEnumerable<RolesModel>? Roles { get; set; }
        public RolesModel? NewRole { get; set; }
        public List<PermissionCheckbox>? Permissions { get; set; } = new List<PermissionCheckbox>();
    }

    public class PermissionCheckbox
    {
        public int PermissionID { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
}
