using LittleArkFoundation.Areas.Admin.Models.Assessments;
using LittleArkFoundation.Areas.Admin.Models.Diagnoses;
using LittleArkFoundation.Areas.Admin.Models.MedicalHistory;

namespace LittleArkFoundation.Areas.Admin.Models.Patients
{
    public class PatientsViewModel
    {
        public List<PatientsModel>? Patients { get; set; }
        public List<AssessmentsModel>? Assessments { get; set; }
        public List<MedicalHistoryModel>? MedicalHistory { get; set; }
        public PatientsModel? Patient { get; set; }
    }
}
