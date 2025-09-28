using LittleArkFoundation.Areas.Admin.Models.Assessments;
using LittleArkFoundation.Areas.Admin.Models.Discharges;
using LittleArkFoundation.Areas.Admin.Models.GeneralAdmission;
using LittleArkFoundation.Areas.Admin.Models.OPD;
using LittleArkFoundation.Areas.Admin.Models.ProgressNotes;

namespace LittleArkFoundation.Areas.Admin.Models.Dashboard
{
    public class DashboardViewModel
    {
        public List<OPDModel> DailyOPD { get; set; } = new List<OPDModel>();
        public List<OPDModel> MonthlyOPD { get; set; } = new List<OPDModel>();
        public List<OPDModel> YearlyOPD { get; set; } = new List<OPDModel>();
        public List<GeneralAdmissionModel> DailyGA { get; set; } = new List<GeneralAdmissionModel>();
        public List<GeneralAdmissionModel> MonthlyGA { get; set; } = new List<GeneralAdmissionModel>();
        public List<GeneralAdmissionModel> YearlyGA { get; set; } = new List<GeneralAdmissionModel>();
        public List<DischargesModel> DailyDischarges { get; set; } = new List<DischargesModel>();
        public List<DischargesModel> MonthlyDischarges { get; set; } = new List<DischargesModel>();
        public List<DischargesModel> YearlyDischarges { get; set; } = new List<DischargesModel>();
        public List<ProgressNotesModel> DailyProgressNotes { get; set; } = new List<ProgressNotesModel>();
        public List<ProgressNotesModel> MonthlyProgressNotes { get; set; } = new List<ProgressNotesModel>();
        public List<ProgressNotesModel> YearlyProgressNotes { get; set; } = new List<ProgressNotesModel>();
        public List<OPDModel> OPDList { get; set; } = new List<OPDModel>();
        public List<GeneralAdmissionModel> GeneralAdmissionsList { get; set; } = new List<GeneralAdmissionModel>();
        public List<DischargesModel> DischargesList { get; set; } = new List<DischargesModel>();
        public List<ProgressNotesModel> ProgressNotesList { get; set; } = new List<ProgressNotesModel>();   
    }
}
