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
            var opdAge = opd.Age?.Trim();

            if (!string.IsNullOrEmpty(opdAge) && System.Text.RegularExpressions.Regex.IsMatch(opdAge, @"\d"))
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(opdAge, @"^\d+$"))
                {
                    var age = int.Parse(opdAge);

                    if (age == 1)
                    {
                        scores.Add($"Age_Baby: {opd.Age}", 10);
                    }
                    else if (age > 1 && age <= 5)
                    {
                        scores.Add($"Age_Child: {opd.Age}", 5);
                    }
                    else if (age > 40)
                    {
                        scores.Add($"Age_Adult: {opd.Age}", 10);
                    }
                }
                else
                {
                    // Any non-pure number, but contains digits (like "6/12", "10 D/O")
                    scores.Add($"Age_Baby: {opd.Age}", 10);
                }
            }
 
            // DIAGNOSIS
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
            var matchedConditions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(opd.Diagnosis))
            {
                var diagnosisText = opd.Diagnosis.ToUpperInvariant();

                foreach (var kvp in diagnosisPatterns)
                {
                    if (matchedConditions.Contains(kvp.Key))
                        continue;

                    foreach (var keyword in kvp.Value.Keywords)
                    {
                        if (diagnosisText.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                        {
                            diagnosisScore += kvp.Value.Weight;
                            matchedConditions.Add(kvp.Key);
                            break; // stop checking more synonyms for this condition
                        }
                    }
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
            var testPatterns = new Dictionary<string, (int Weight, string[] Keywords)>(StringComparer.OrdinalIgnoreCase)
            {
                { "BUN", (5, new[] { "BUN", "BLOOD UREA NITROGEN", "UREA" }) },
                { "CREA", (5, new[] { "CREA", "CREATININE" }) },
                // You can add more here easily:
                // { "CBC", (3, new[] { "CBC", "COMPLETE BLOOD COUNT" }) },
                // { "FBS", (3, new[] { "FBS", "FASTING BLOOD SUGAR", "FASTING GLUCOSE" }) },
                // { "LIPID PROFILE", (2, new[] { "LIPID", "CHOLESTEROL", "TRIGLYCERIDES" }) },
                // { "UA", (1, new[] { "URINALYSIS", "URINE ANALYSIS", "UA" }) }
            };

            if (!string.IsNullOrWhiteSpace(opd.AssistanceNeeded))
            {
                var assistanceText = opd.AssistanceNeeded.ToUpperInvariant();
                var matchedTests = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var kvp in testPatterns)
                {
                    if (matchedTests.Contains(kvp.Key))
                        continue;

                    foreach (var keyword in kvp.Value.Keywords)
                    {
                        if (assistanceText.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                        {
                            scores.TryAdd($"Assistance_{kvp.Key}: {opd.AssistanceNeeded}", kvp.Value.Weight);
                            matchedTests.Add(kvp.Key);
                            break; // prevent double-counting
                        }
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
            return totalScore >= 25; 
        }
    }
}
