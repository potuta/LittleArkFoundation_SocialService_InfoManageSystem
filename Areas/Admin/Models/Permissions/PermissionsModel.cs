namespace LittleArkFoundation.Areas.Admin.Models
{
    public class PermissionsModel
    {
        public int PermissionID { get; set; }
        public string Name { get; set; }
        public string? PermissionType { get; set; }
        public string? Module { get; set; }
    }
}
