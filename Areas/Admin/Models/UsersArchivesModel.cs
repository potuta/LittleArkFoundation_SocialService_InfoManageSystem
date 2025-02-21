namespace LittleArkFoundation.Areas.Admin.Models
{
    public class UsersArchivesModel
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public int RoleID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ArchivedAt { get; set; }
        public string ArchivedBy { get; set; }
    }
}
