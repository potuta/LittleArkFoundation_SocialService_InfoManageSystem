namespace LittleArkFoundation.Areas.Admin.Models
{
    public class UsersViewModel
    {
        public IEnumerable<UsersModel>? Users { get; set; }
        public IEnumerable<UsersArchivesModel>? UsersArchives { get; set; }
        public UsersModel? NewUser { get; set; }
        public UsersArchivesModel? NewUserArchive { get; set; }
        public IEnumerable<RolesModel>? Roles { get; set; }
        public string? DefaultUserPassword { get; set; }
        public int? AdminCount { get; set; }
    }
}