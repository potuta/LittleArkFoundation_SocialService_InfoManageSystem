using LittleArkFoundation.Areas.Admin.Models.Patients;

namespace LittleArkFoundation.Areas.Admin.Models.Form
{
    public class FormViewModel
    {
        public List<FormResponsesModel>? FormResponses { get; set; }
        public FormResponsesModel? NewForm { get; set; }
        public PatientsModel? Patient { get; set; }
    }
}
