namespace LittleArkFoundation.Areas.Admin.Models.Discharges
{
    public class DischargesModel
    {
        public int Id { get; set; }
        public int AssessmentID { get; set; }
        public int PatientID { get; set; }
        public DateOnly ProcessedDate { get; set; }
        public DateOnly DischargedDate { get; set; }
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string LastName { get; set; }
        public string? Suffix { get; set; }
        public string Ward { get; set; }
        public TimeOnly ReceivedHB { get; set; }
        public TimeOnly IssuedMSS { get; set; }
        public string Duration { get; set; }
        public string Class { get; set; }
        public string PHICCategory { get; set; }
        public bool PHICUsed { get; set; }
        public string RemarksIfNo { get; set; }
        public string MSW { get; set; }
        public int UserID { get; set; }
    }
}
