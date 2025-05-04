namespace LittleArkFoundation.Areas.Admin.Models.Discharges
{
    public class DischargesModel
    {
        public int Id { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public DateOnly ProcessedDate { get; set; } = new DateOnly(1900, 1, 1);
        public DateOnly DischargedDate { get; set; } = new DateOnly(1900, 1, 1);
        public string FirstName { get; set; } = "N/A";
        public string MiddleName { get; set; } = "N/A";
        public string LastName { get; set; } = "N/A";
        public string Ward { get; set; } = "N/A";
        public TimeOnly ReceivedHB { get; set; } = new TimeOnly(0, 0, 0);
        public TimeOnly IssuedMSS { get; set; } = new TimeOnly(0, 0, 0);
        public string Duration { get; set; } = "N/A";
        public string Class { get; set; } = "N/A";
        public string PHICCategory { get; set; } = "N/A";
        public bool PHICUsed { get; set; } = false;
        public string RemarksIfNo { get; set; } = "N/A";
        public string MSW { get; set; } = "N/A";
        public int UserID { get; set; } = 0;
    }
}
