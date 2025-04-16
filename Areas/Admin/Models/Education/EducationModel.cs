namespace LittleArkFoundation.Areas.Admin.Models.Education
{
    public class EducationModel
    {
        public int EducationID { get; set; }
        public int PatientID { get; set; }
        public bool IsCurrentlyEnrolled { get; set; } = false;
        public string SchoolName { get; set; } = "N/A";
        public string ChildGradeLevel { get; set; } = "N/A";
        public string SummerGradeLevel { get; set; } = "N/A";
        public string DescribeChildAttendance { get; set; } = "N/A";
        public string ChildAttendance { get; set; } = "N/A";
        public string DescribeChildAchievements { get; set; } = "N/A";
        public string DescribeChildAttitude { get; set; } = "N/A";
        public bool HasDisciplinaryIssues { get; set; } = false;
        public string DescribeDisciplinaryIssues { get; set; } = "N/A";
        public bool HasSpecialEducation { get; set; } = false;
        public string DescribeSpecialEducation { get; set; } = "N/A";
        public bool HasHomeStudy { get; set; } = false;
        public string DescribeHomeStudy { get; set; } = "N/A";
        public bool HasDiagnosedLearningDisability { get; set; } = false;
        public string DescribeDiagnosedLearningDisability { get; set; } = "N/A";
        public bool HasSpecialServices { get; set; } = false;
        public string DescribeSpecialServices { get; set; } = "N/A";
    }
}
