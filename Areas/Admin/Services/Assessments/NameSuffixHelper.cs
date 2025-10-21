using System.Text;

namespace LittleArkFoundation.Areas.Admin.Services.Assessments
{
    public class NameSuffixHelper
    {
        public static List<string> GetSuffixes(int max = 10)
        {
            string ToRoman(int number)
            {
                var map = new Dictionary<int, string>()
                {
                    {1000, "M"}, {900, "CM"}, {500, "D"}, {400, "CD"},
                    {100, "C"}, {90, "XC"}, {50, "L"}, {40, "XL"},
                    {10, "X"}, {9, "IX"}, {5, "V"}, {4, "IV"}, {1, "I"}
                };

                var result = new StringBuilder();
                foreach (var pair in map)
                {
                    while (number >= pair.Key)
                    {
                        result.Append(pair.Value);
                        number -= pair.Key;
                    }
                }
                return result.ToString();
            }

            var suffixes = new List<string> { "Jr.", "Sr." };

            for (int i = 1; i <= max; i++)
            {
                suffixes.Add(ToRoman(i));
            }

            // Optional professional titles
            suffixes.AddRange(new List<string>
            {
                "MD", "PhD", "DDS", "Esq.", "CPA", "RN"
            });

            return suffixes;
        }
    }
}
