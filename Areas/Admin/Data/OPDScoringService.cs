using LittleArkFoundation.Areas.Admin.Models.Criteria;
using LittleArkFoundation.Areas.Admin.Models.OPD;
using LittleArkFoundation.Data;
using Microsoft.EntityFrameworkCore;
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
                ["Diagnosis"] = (0, "No diagnosis matched"),
                ["Assistance Needed"] = (0, "No assistance matched")
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

            var criteriaList = await context.Criteria.ToListAsync();

            var criteriaDiagnosisResult = CalculateDiagnosisScore(opd.Diagnosis, criteriaList);
            scores["Diagnosis"] = (criteriaDiagnosisResult.Score, criteriaDiagnosisResult.Description);

            var assistanceResult = CalculateAssistanceScore(opd.AssistanceNeeded, criteriaList);
            scores["Assistance Needed"] = (assistanceResult.Score, assistanceResult.Description); 

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

        private (int Score, string Description) CalculateDiagnosisScore(string diagnosisText, List<CriteriaModel> criteriaList)
        {
            int diagnosisScore = 0;
            var matchedConditions = new List<string>();

            if (string.IsNullOrWhiteSpace(diagnosisText))
                return (0, "No diagnosis provided");

            var diagnosisGroups = criteriaList
                .GroupBy(c => c.DiagnosisID)
                .Select(g => new
                {
                    DisplayName = g.FirstOrDefault(x => x.IsDisplayName)?.Diagnosis,
                    Weight = g.FirstOrDefault(x => x.IsDisplayName)?.Weight ?? 0,
                    Keywords = g.Select(x => x.Diagnosis).ToList()
                })
                .ToList();

            var text = diagnosisText.ToUpperInvariant();

            foreach (var group in diagnosisGroups)
            {
                foreach (var keyword in group.Keywords)
                {
                    if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        diagnosisScore += group.Weight;
                        matchedConditions.Add(group.DisplayName);
                        break;
                    }
                }
            }

            string diagnosisDescription;

            if (matchedConditions.Count > 0)
            {
                diagnosisDescription = $"Matched: {string.Join(", ", matchedConditions)} ({diagnosisText})";
            }
            else
            {
                diagnosisDescription = "No diagnosis matched";
            }

            return (diagnosisScore, diagnosisDescription);
        }

        private (int Score, string Description) CalculateAssistanceScore(string assistanceText, List<CriteriaModel> criteriaList)
        {
            int assistanceScore = 0;
            var matchedTests = new List<string>();

            if (string.IsNullOrWhiteSpace(assistanceText))
                return (0, "No assistance provided");

            var assistanceGroups = criteriaList
                .GroupBy(c => c.DiagnosisID)
                .Select(g => new
                {
                    DisplayName = g.FirstOrDefault(x => x.IsDisplayName)?.Diagnosis,
                    Weight = g.FirstOrDefault(x => x.IsDisplayName)?.Weight ?? 0,
                    Keywords = g.Where(x => !x.IsDisplayName)
                                .Select(x => x.Diagnosis)
                                .ToList()
                })
                .ToList();

            var text = assistanceText.ToUpperInvariant();

            foreach (var group in assistanceGroups)
            {
                foreach (var keyword in group.Keywords)
                {
                    if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        assistanceScore += group.Weight;
                        matchedTests.Add(group.DisplayName);
                        break;
                    }
                }
            }

            string description;

            if (matchedTests.Count > 0)
            {
                description = $"Matched: {string.Join(", ", matchedTests)} ({assistanceText})";
            }
            else
            {
                description = "No assistance matched";
            }

            return (assistanceScore, description);
        }
    }
}
