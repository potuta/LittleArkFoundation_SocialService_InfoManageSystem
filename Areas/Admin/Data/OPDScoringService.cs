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
            if (opd.MonthlyIncome <= 20000)
            {
                scores.Add($"MonthlyIncome_Low: {opd.MonthlyIncome.ToString()}", 10);
            }
            else if (opd.MonthlyIncome > 20000 && opd.MonthlyIncome <= 35000)
            {
                scores.Add($"MonthlyIncome_Moderate: {opd.MonthlyIncome.ToString()}", 5);
            }
            else if (opd.MonthlyIncome > 35000)
            {
                scores.Add($"MonthlyIncome_High: {opd.MonthlyIncome.ToString()}", 1);
            }

            // AMOUNT REQUESTED
            if (opd.Amount <= 5000)
            {
                scores.Add($"Amount_Low: {opd.Amount.ToString()}", 10);
            }
            else if (opd.Amount > 5000 && opd.Amount <= 10000)
            {
                scores.Add($"Amount_Moderate: {opd.Amount.ToString()}", 5);
            }
            else if (opd.Amount > 10000)
            {
                scores.Add($"Amount_High: {opd.Amount.ToString()}", 1);
            }

            // NO OF CHILDREN
            if (opd.NoOfChildren > 3)
            {
                scores.Add($"NoOfChildren: {opd.NoOfChildren.ToString()}", 5);
            }

            // PWD (Person with Disability)
            if (opd.IsPWD)
            {
                scores.Add($"IsPWD: {opd.IsPWD.ToString()}", 20);
            }

            // MINOR (Age < 18)
            if (opd.Age < 18)
            {
                scores.Add($"Age_Minor: {opd.Age.ToString()}", 10);
            }
            else if (opd.Age >= 18 && opd.Age <= 60)
            {
                scores.Add($"Age_Adult: {opd.Age.ToString()}", 5);
            }
            else if (opd.Age > 60)
            {
                scores.Add($"Age_Senior: {opd.Age.ToString()}", 10);
            }

            // DIAGNOSIS
            var diagnosisList = new List<string>
            {
                "AGN", "DENGUE", "TUBERCULOSIS"
            };

            var diagnosisParts = opd.Diagnosis.ToUpper().Split(',', StringSplitOptions.RemoveEmptyEntries);
            int diagnosisScore = 0;

            foreach (var part in diagnosisParts)
            {
                if (diagnosisList.Contains(part.Trim()))
                {
                    diagnosisScore += 10; // 10 per critical diagnosis
                }
            }

            if (diagnosisScore > 0)
            {
                scores.Add("Diagnosis_Critical", diagnosisScore);
            }

            // ASSISTANCE REQUESTED
            var highPriorityTests = new[] { "BUN", "ALT", "K", "NA", "CA", "CREA", "AST" };
            var midPriorityTests = new[] { "CBC", "FBS", "LIPID PROFILE" };
            var lowPriorityTests = new[] { "URIC ACID", "UA" };

            if (!string.IsNullOrEmpty(opd.AssistanceNeeded))
            {
                var requestedTests = opd.AssistanceNeeded.ToUpper().Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var test in requestedTests.Select(t => t.Trim()))
                {
                    if (highPriorityTests.Contains(test))
                    {
                        scores.TryAdd($"Assistance_{test}", 10);
                    }
                    else if (midPriorityTests.Contains(test))
                    {
                        scores.TryAdd($"Assistance_{test}", 5);
                    }
                    else if (lowPriorityTests.Contains(test))
                    {
                        scores.TryAdd($"Assistance_{test}", 3);
                    }
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
            return totalScore >= 35; // Assuming 35 is the threshold for eligibility
        }
    }
}
