namespace LittleArkFoundation.Areas.Admin.Models
{
    public class RolesModel
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public List<RolePermissionsModel> RolePermissions { get; set; } = new List<RolePermissionsModel>();
    }
}
