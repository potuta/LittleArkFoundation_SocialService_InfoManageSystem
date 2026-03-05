using System.ComponentModel.DataAnnotations;

namespace LittleArkFoundation.Areas.Admin.Models.Criteria
{
    public class CriteriaModel
    {
        public int Id { get; set; }
        public int DiagnosisID { get; set; }

        [Required]
        public string Diagnosis { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Weight must be a positive integer")]
        public int Weight { get; set; }
        public bool IsDisplayName { get; set; }
    }
}
