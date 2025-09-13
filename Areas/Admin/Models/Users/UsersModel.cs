using System.ComponentModel.DataAnnotations.Schema;

namespace LittleArkFoundation.Areas.Admin.Models
{
    public class UsersModel
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public string? PasswordSalt { get; set; }
        public int RoleID { get; set; }
        public DateTime CreatedAt { get; set; }
        public byte[]? ProfilePicture { get; set; }
        public string? ProfilePictureContentType { get; set; } = "application/octet-stream";

        [NotMapped]
        public IFormFile? ProfilePictureFile { get; set; }
    }
}