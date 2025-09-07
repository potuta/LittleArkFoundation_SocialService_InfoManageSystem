using ClosedXML.Excel;
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

        public static void ApplyWorksheetStatistics(
            IXLWorksheet worksheet,
            int row,
            string title,
            int indent,
            Dictionary<int, Dictionary<string, int>> totalStatisticsMonthlyDictionary,
            string key,
            bool isWrapText = false
            )
        {
            worksheet.Cell(row, 1).Value = title;
            worksheet.Cell(row, 1).Style.Alignment.WrapText = isWrapText;
            worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(row, 1).Style.Alignment.Indent = indent;

            for (int i = 1; i <= 6; i++)
            {
                var count = totalStatisticsMonthlyDictionary[i][key];
                worksheet.Cell(row, i + 1).Value = count == 0 ? "" : count;
                worksheet.Cell(row, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(row, 8).Value =
                Enumerable.Range(1, 6).Sum(i => totalStatisticsMonthlyDictionary[i][key]);
            worksheet.Cell(row, 8).Style.Font.Bold = true;
            worksheet.Cell(row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int i = 7; i <= 12; i++)
            {
                var count = totalStatisticsMonthlyDictionary[i][key];
                worksheet.Cell(row, i + 2).Value = count == 0 ? "" : count;
                worksheet.Cell(row, i + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            worksheet.Cell(row, 15).Value =
                Enumerable.Range(7, 6).Sum(i => totalStatisticsMonthlyDictionary[i][key]);
            worksheet.Cell(row, 15).Style.Font.Bold = true;
            worksheet.Cell(row, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        }
    }
}
