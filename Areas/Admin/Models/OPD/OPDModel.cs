namespace LittleArkFoundation.Areas.Admin.Models.OPD
{
    public class OPDModel
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; } = DateOnly.MinValue;
        public bool IsOld { get; set; } = false;
        public bool IsAdmitted { get; set; } = false;
        public string Class { get; set; } = "N/A";
        public string FirstName { get; set; } = "N/A";
        public string MiddleName { get; set; } = "N/A";
        public string LastName { get; set; } = "N/A";
        public string ContactNo { get; set; } = "0";
        public string Age { get; set; } = "0";
        public string Gender { get; set; } = "N/A";
        public bool IsPWD { get; set; } = false;
        public string Diagnosis { get; set; }   = "N/A";
        public string Address { get; set; } = "N/A";
        public string SourceOfReferral { get; set; } = "N/A";
        public string MotherFirstName { get; set; } = "N/A";
        public string MotherMiddleName { get; set; } = "N/A";
        public string MotherLastName { get; set; } = "N/A";
        public string MotherOccupation { get; set; } = "N/A";
        public string FatherFirstName { get; set; } = "N/A";
        public string FatherMiddleName { get; set; } = "N/A";
        public string FatherLastName { get; set; } = "N/A";
        public string FatherOccupation { get; set; } = "N/A";
        public decimal MonthlyIncome { get; set; } = 0.00m;
        public int NoOfChildren { get; set; } = 0;
        public string AssistanceNeeded { get; set; } = "N/A";
        public decimal Amount { get; set; } = 0.00m;
        public string PtShare { get; set; } = "N/A";
        public decimal AmountExtended { get; set; } = 0.00m;
        public string Resources { get; set; } = "N/A";
        public string GLProponent { get; set; } = "N/A";
        public decimal GLAmountReceived { get; set; } = 0.00m;
        public string MSW { get; set; } = "N/A";
        public string Category { get; set; } = "N/A";
        public int UserID { get; set; } = 0;

    }
}
