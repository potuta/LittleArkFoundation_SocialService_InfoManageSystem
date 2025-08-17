using LittleArkFoundation.Areas.Admin.Models.OPD;
using LittleArkFoundation.Data;
using System.Data;

namespace LittleArkFoundation.Areas.Admin.Data
{
    public class OPDScoringService
    {
        private readonly string _connectionString;

        public OPDScoringService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Dictionary<string, int>> GetWeightedScoresAsync(OPDModel opd)
        {
            var scores = new Dictionary<string, int>();

            await using var context = new ApplicationDbContext(_connectionString);

            // MONTHLY INCOME
            //if (opd.MonthlyIncome <= 20000)
            //{
            //    scores.Add($"MonthlyIncome_Low: {opd.MonthlyIncome.ToString()}", 10);
            //}
            //else if (opd.MonthlyIncome > 20000 && opd.MonthlyIncome <= 35000)
            //{
            //    scores.Add($"MonthlyIncome_Moderate: {opd.MonthlyIncome.ToString()}", 5);
            //}
            //else if (opd.MonthlyIncome > 35000)
            //{
            //    scores.Add($"MonthlyIncome_High: {opd.MonthlyIncome.ToString()}", 1);
            //}

            // AMOUNT REQUESTED
            //if (opd.Amount <= 5000)
            //{
            //    scores.Add($"Amount_Low: {opd.Amount.ToString()}", 10);
            //}
            //else if (opd.Amount > 5000 && opd.Amount <= 10000)
            //{
            //    scores.Add($"Amount_Moderate: {opd.Amount.ToString()}", 5);
            //}
            //else if (opd.Amount > 10000)
            //{
            //    scores.Add($"Amount_High: {opd.Amount.ToString()}", 1);
            //}

            // NO OF CHILDREN
            //if (opd.NoOfChildren > 3)
            //{
            //    scores.Add($"NoOfChildren: {opd.NoOfChildren.ToString()}", 5);
            //}

            // PWD (Person with Disability)
            //if (opd.IsPWD)
            //{
            //    scores.Add($"IsPWD: {opd.IsPWD.ToString()}", 20);
            //}

            // AGE
            if (opd.Age == 1)
            {
                scores.Add($"Age_Baby: {opd.Age.ToString()}", 10);
            }
            else if (opd.Age > 1 && opd.Age <= 5)
            {
                scores.Add($"Age_Child: {opd.Age.ToString()}", 5);
            }
            else if (opd.Age > 40)
            {
                scores.Add($"Age_Adult: {opd.Age.ToString()}", 10);
            }

            // DIAGNOSIS
            var diagnosisWeights = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "AGN", 5 },
                { "PSGN", 5 },
                { "AZOTEMIA", 10 },
                { "NEPHROTIC SYNDROME", 15 },
                { "EDEMA", 15 },
                { "OLIGURIA", 20 },
                { "HYPERTENSION", 25 }
            };

            int diagnosisScore = 0;

            foreach (var part in opd.Diagnosis.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var cleaned = part.Trim();
                if (diagnosisWeights.TryGetValue(cleaned, out int weight))
                {
                    diagnosisScore += weight;
                }
            }

            if (diagnosisScore > 0)
            {
                string text = "";

                if (diagnosisScore >= 25)
                {
                    text = "Diagnosis_Critical";
                }
                else if (diagnosisScore >= 15 && diagnosisScore < 25)
                {
                    text = "Diagnosis_Moderate";
                }
                else if (diagnosisScore < 15)
                {
                    text = "Diagnosis_Mild";
                }

                scores.Add($"{text}: {opd.Diagnosis}", diagnosisScore);
            }

            // ASSISTANCE REQUESTED
            var highPriorityTests = new[] { "BUN", "CREA" };
            //var midPriorityTests = new[] { "CBC", "FBS", "LIPID PROFILE" };
            //var lowPriorityTests = new[] { "URIC ACID", "UA" };

            if (!string.IsNullOrEmpty(opd.AssistanceNeeded))
            {
                var requestedTests = opd.AssistanceNeeded.ToUpper().Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var test in requestedTests.Select(t => t.Trim()))
                {
                    if (highPriorityTests.Contains(test))
                    {
                        scores.TryAdd($"Assistance_{test}", 5);
                    }
                    //else if (midPriorityTests.Contains(test))
                    //{
                    //    scores.TryAdd($"Assistance_{test}", 5);
                    //}
                    //else if (lowPriorityTests.Contains(test))
                    //{
                    //    scores.TryAdd($"Assistance_{test}", 3);
                    //}
                }
            }

            return scores;
        }

        public async Task<int> GetTotalWeightedScoreAsync(OPDModel opd)
        {
            var scores = await GetWeightedScoresAsync(opd);
            return scores.Values.Sum();
        }

        public async Task<bool> IsEligibleForAdmissionAsync(int totalScore)
        {
            return totalScore >= 25; 
        }
    }
}
