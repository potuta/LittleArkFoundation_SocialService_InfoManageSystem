using LittleArkFoundation.Areas.Admin.Models.Assessments;
using LittleArkFoundation.Areas.Admin.Models.Diagnoses;
using LittleArkFoundation.Areas.Admin.Models.Discharges;
using LittleArkFoundation.Areas.Admin.Models.MedicalHistory;
using LittleArkFoundation.Areas.Admin.Models.MSWDClassification;

namespace LittleArkFoundation.Areas.Admin.Models.Patients
{
    public class PatientsViewModel
    {
        public List<PatientsModel> Patients { get; set; } = new List<PatientsModel>() { new PatientsModel() };
        public List<AssessmentsModel> Assessments { get; set; } = new List<AssessmentsModel> { new AssessmentsModel() };
        public List<MedicalHistoryModel> MedicalHistory { get; set; } = new List<MedicalHistoryModel> { new MedicalHistoryModel() };
        public PatientsModel Patient { get; set; } = new PatientsModel();
        public DischargesModel Discharge { get; set; } = new DischargesModel();
        public List<MSWDClassificationModel> MSWDClassifications { get; set; } = new List<MSWDClassificationModel> { new MSWDClassificationModel() };
        public List<UsersModel> Users { get; set; } = new List<UsersModel> { new UsersModel() };
    }
}
