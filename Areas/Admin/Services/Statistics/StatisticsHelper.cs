using LittleArkFoundation.Areas.Admin.Models.Statistics;

namespace LittleArkFoundation.Areas.Admin.Services.Statistics
{
    public class StatisticsHelper
    {
        public static Dictionary<string, int> SumForMonth(IEnumerable<StatisticsModel> stats, int month)
        {
            var props = typeof(StatisticsModel)
                .GetProperties()
                .Where(p =>
                    p.PropertyType == typeof(int?) &&
                    (p.Name.StartsWith("i_") || p.Name.StartsWith("ii_") || p.Name.StartsWith("iii_")) &&
                    !string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(p.Name, "UserID", StringComparison.OrdinalIgnoreCase)
                );

            var monthStats = stats.Where(s => s.Date.HasValue && s.Date.Value.Month == month);

            var result = new Dictionary<string, int>();

            foreach (var prop in props)
            {
                result[prop.Name] = monthStats.Sum(s => (int?)(prop.GetValue(s) ?? 0)) ?? 0;
            }

            return result;
        }
    }
}
