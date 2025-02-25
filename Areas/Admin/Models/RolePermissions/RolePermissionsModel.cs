using System.ComponentModel.DataAnnotations.Schema;

namespace LittleArkFoundation.Areas.Admin.Models
{
    public class RolePermissionsModel
    {
        public int Id { get; set; }

        [ForeignKey("Role")]
        public int RoleID { get; set; }
        public RolesModel Role { get; set; }

        [ForeignKey("Permissions")]
        public int PermissionID { get; set; }
        public PermissionsModel Permissions { get; set; }
    }
}
