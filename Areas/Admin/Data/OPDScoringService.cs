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

        public async Task<Dictionary<string, (int Score, string Description)>> GetWeightedScoresAsync(OPDModel opd)
        {
            var scores = new Dictionary<string, (int Score, string Description)>
            {
                ["Age"] = (0, "No match"),
                //["MonthlyIncome"] = (0, "No match"),
                //["AmountRequested"] = (0, "No match"),
                //["NoOfChildren"] = (0, "No match"),
                //["IsPWD"] = (0, "No match"),
                ["Diagnosis"] = (0, "No diagnosis matched"),
                ["AssistanceNeeded"] = (0, "No assistance matched")
            };

            await using var context = new ApplicationDbContext(_connectionString);

            // --- AGE ---
            var opdAge = opd.Age?.Trim();
            if (!string.IsNullOrEmpty(opdAge) && System.Text.RegularExpressions.Regex.IsMatch(opdAge, @"\d"))
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(opdAge, @"^\d+$"))
                {
                    var age = int.Parse(opdAge);

                    if (age == 1)
                        scores["Age"] = (10, "Baby (age = 1)");
                    else if (age > 1 && age <= 5)
                        scores["Age"] = (5, "Child (age between 2–5)");
                    else if (age > 40)
                        scores["Age"] = (10, "Adult (age > 40)");
                }
                else
                {
                    scores["Age"] = (10, $"Infant (non-numeric age: {opd.Age})");
                }
            }

            // --- MONTHLY INCOME ---
            //if (opd.MonthlyIncome <= 20000)
            //    scores["MonthlyIncome"] = (10, "Low income (≤ 20,000)");
            //else if (opd.MonthlyIncome > 20000 && opd.MonthlyIncome <= 35000)
            //    scores["MonthlyIncome"] = (5, "Moderate income (20,001 – 35,000)");
            //else if (opd.MonthlyIncome > 35000)
            //    scores["MonthlyIncome"] = (1, "High income (> 35,000)");

            // --- AMOUNT REQUESTED ---
            //if (opd.Amount <= 5000)
            //    scores["AmountRequested"] = (10, "Low amount (≤ 5,000)");
            //else if (opd.Amount > 5000 && opd.Amount <= 10000)
            //    scores["AmountRequested"] = (5, "Moderate amount (5,001 – 10,000)");
            //else if (opd.Amount > 10000)
            //    scores["AmountRequested"] = (1, "High amount (> 10,000)");

            // --- NO OF CHILDREN ---
            //if (opd.NoOfChildren > 3)
            //    scores["NoOfChildren"] = (5, $"{opd.NoOfChildren} children (> 3)");

            // --- PWD ---
            //if (opd.IsPWD)
            //    scores["IsPWD"] = (20, "Person with Disability (PWD)");

            // --- DIAGNOSIS ---
            var diagnosisPatterns = new Dictionary<string, (int Weight, string[] Keywords)>(StringComparer.OrdinalIgnoreCase)
            {
                { "AGN", (5, new[] { "AGN", "ACUTE GLOMERULONEPHRITIS", "GLOMERULONEPHRITIS" }) },
                { "PSGN", (5, new[] { "PSGN", "POST STREPTOCOCCAL GLOMERULONEPHRITIS", "POST-STREPTOCOCCAL GLOMERULONEPHRITIS" }) },
                { "AZOTEMIA", (10, new[] { "AZOTEMIA" }) },
                { "NEPHROTIC SYNDROME", (15, new[] { "NEPHROTIC", "NEPHROTIC SYNDROME", "NEPHROSIS" }) },
                { "EDEMA", (15, new[] { "EDEMA", "OEDEMA" }) },
                { "OLIGURIA", (20, new[] { "OLIGURIA" }) },
                { "HYPERTENSION", (25, new[] { "HYPERTENSION", "HTN", "HIGH BLOOD PRESSURE" }) }
            };

            int diagnosisScore = 0;
            var matchedConditions = new List<string>();

            if (!string.IsNullOrWhiteSpace(opd.Diagnosis))
            {
                var diagnosisText = opd.Diagnosis.ToUpperInvariant();

                foreach (var kvp in diagnosisPatterns)
                {
                    foreach (var keyword in kvp.Value.Keywords)
                    {
                        if (diagnosisText.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                        {
                            diagnosisScore += kvp.Value.Weight;
                            matchedConditions.Add(kvp.Key);
                            break;
                        }
                    }
                }
            }

            string diagnosisDescription;
            if (matchedConditions.Count > 0)
            {
                diagnosisDescription = $"Matched: {string.Join(", ", matchedConditions)} ({opd.Diagnosis})";
            }
            else
            {
                diagnosisDescription = "No diagnosis matched";
            }

            scores["Diagnosis"] = (diagnosisScore, diagnosisDescription);

            // --- ASSISTANCE REQUESTED ---
            var testPatterns = new Dictionary<string, (int Weight, string[] Keywords)>(StringComparer.OrdinalIgnoreCase)
            {
                { "BUN", (5, new[] { "BUN", "BLOOD UREA NITROGEN", "UREA" }) },
                { "CREA", (5, new[] { "CREA", "CREATININE" }) }
            };

            int assistanceScore = 0;
            var matchedTests = new List<string>();

            if (!string.IsNullOrWhiteSpace(opd.AssistanceNeeded))
            {
                var assistanceText = opd.AssistanceNeeded.ToUpperInvariant();

                foreach (var kvp in testPatterns)
                {
                    foreach (var keyword in kvp.Value.Keywords)
                    {
                        if (assistanceText.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                        {
                            assistanceScore += kvp.Value.Weight;
                            matchedTests.Add(kvp.Key);
                            break;
                        }
                    }
                }
            }

            string assistanceDescription = matchedTests.Count > 0
                ? $"Matched: {string.Join(", ", matchedTests)} ({opd.AssistanceNeeded})"
                : "No assistance matched";

            scores["AssistanceNeeded"] = (assistanceScore, assistanceDescription);

            return scores;
        }


        public async Task<int> GetTotalWeightedScoreAsync(OPDModel opd)
        {
            var scores = await GetWeightedScoresAsync(opd);
            return scores.Values.Sum(x => x.Score);    
        }

        public async Task<bool> IsEligibleForAdmissionAsync(int totalScore)
        {
            return totalScore >= 25; 
        }
    }
}
