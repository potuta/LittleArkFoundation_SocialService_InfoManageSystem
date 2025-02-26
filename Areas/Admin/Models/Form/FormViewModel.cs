using LittleArkFoundation.Areas.Admin.Models.Assessments;
using LittleArkFoundation.Areas.Admin.Models.FamilyComposition;
using LittleArkFoundation.Areas.Admin.Models.Informants;
using LittleArkFoundation.Areas.Admin.Models.Patients;
using LittleArkFoundation.Areas.Admin.Models.Referrals;

namespace LittleArkFoundation.Areas.Admin.Models.Form
{
    public class FormViewModel
    {
        public AssessmentsModel? Assessments { get; set; }
        public ReferralsModel? Referrals { get; set; }
        public InformantsModel? Informants { get; set; }
        public PatientsModel? Patient { get; set; }
        public List<FamilyCompositionModel>? FamilyMembers { get; set; }
        public FamilyCompositionModel? FamilyComposition { get; set; }

    }
}
