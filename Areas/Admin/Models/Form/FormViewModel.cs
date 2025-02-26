using LittleArkFoundation.Areas.Admin.Models.Assessments;
using LittleArkFoundation.Areas.Admin.Models.Informants;
using LittleArkFoundation.Areas.Admin.Models.Patients;
using LittleArkFoundation.Areas.Admin.Models.Referrals;

namespace LittleArkFoundation.Areas.Admin.Models.Form
{
    public class FormViewModel
    {
        public List<FormResponsesModel>? FormResponses { get; set; }
        public FormResponsesModel? NewForm { get; set; }
        public PatientsModel? Patient { get; set; }
        public AssessmentsModel? Assessments { get; set; }
        public ReferralsModel? Referrals { get; set; }
        public InformantsModel? Informants { get; set; }
    }
}
