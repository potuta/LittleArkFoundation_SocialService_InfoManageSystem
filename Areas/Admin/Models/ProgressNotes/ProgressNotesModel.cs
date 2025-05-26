using System.ComponentModel.DataAnnotations.Schema;

namespace LittleArkFoundation.Areas.Admin.Models.ProgressNotes
{
    public class ProgressNotesModel
    {
        public int ProgressNotesID { get; set; }
        public int PatientID { get; set; }
        public int AssessmentID { get; set; }
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public string ProgressNotes { get; set; } = "N/A";
        public byte[]? Attachment { get; set; }
        public string? AttachmentContentType { get; set; } = "application/octet-stream";
        public int UserID { get; set; } = 0;

        [NotMapped]
        public IFormFile? AttachmentFile { get; set; }

        [NotMapped]
        public bool RemoveAttachment { get; set; } = false;

    }
}
