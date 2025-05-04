using LittleArkFoundation.Areas.Admin.Models.Assessments;
using LittleArkFoundation.Areas.Admin.Models.Diagnoses;
using LittleArkFoundation.Areas.Admin.Models.MedicalHistory;

namespace LittleArkFoundation.Areas.Admin.Models.Patients
{
    public class PatientsViewModel
    {
        public List<PatientsModel> Patients { get; set; } = new List<PatientsModel>() { new PatientsModel() };
        public List<AssessmentsModel> Assessments { get; set; } = new List<AssessmentsModel> { new AssessmentsModel() };
        public List<MedicalHistoryModel> MedicalHistory { get; set; } = new List<MedicalHistoryModel> { new MedicalHistoryModel() };
        public PatientsModel Patient { get; set; } = new PatientsModel();
    }
}
