namespace LittleArkFoundation.Areas.Admin.Models.OPD
{
    public class OPDModel
    {
        public int Id { get; set; }
        public int OPDId { get; set; } = 0;
        public int PatientID { get; set; } = 0;
        public int AssessmentID { get; set; } = 0;
        public DateOnly Date { get; set; } = DateOnly.MinValue;
        public bool IsOld { get; set; } = false;
        public bool IsAdmitted { get; set; } = false;
        public string Class { get; set; }
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string LastName { get; set; }
        public string? Suffix { get; set; }
        public string ContactNo { get; set; } 
        public string Age { get; set; }
        public string Gender { get; set; } 
        public bool IsPWD { get; set; } = false;
        public string Diagnosis { get; set; }
        public string Address { get; set; }
        public string SourceOfReferral { get; set; }
        public string MotherFirstName { get; set; }
        public string? MotherMiddleName { get; set; }
        public string MotherLastName { get; set; }
        public string? MotherSuffix { get; set; }
        public string MotherOccupation { get; set; }
        public string FatherFirstName { get; set; }
        public string? FatherMiddleName { get; set; }
        public string FatherLastName { get; set; }
        public string? FatherSuffix { get; set; }
        public string FatherOccupation { get; set; }
        public decimal MonthlyIncome { get; set; }
        public int NoOfChildren { get; set; }
        public string AssistanceNeeded { get; set; }
        public decimal Amount { get; set; }
        public string PtShare { get; set; }
        public decimal AmountExtended { get; set; }
        public string Resources { get; set; }
        public string GLProponent { get; set; }
        public decimal GLAmountReceived { get; set; }
        public string MSW { get; set; } = "Unknown";
        public string Category { get; set; }
        public int UserID { get; set; } = 0;

    }
}
