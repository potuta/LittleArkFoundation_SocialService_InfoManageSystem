using HtmlAgilityPack;

namespace LittleArkFoundation.Areas.Admin.Services.Html
{
    public static class HtmlSafeHelper
    {
        // STRING
        public static string Safe(this string? value, string defaultValue = "")
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            if (value.Trim().Equals("N/A", StringComparison.OrdinalIgnoreCase))
                return defaultValue;

            return value!;
        }

        // INT (nullable and non-nullable)
        public static string Safe(this int value, string defaultValue = "")
            => value.ToString();

        public static string Safe(this int? value, string defaultValue = "")
            => value.HasValue ? value.Value.ToString() : defaultValue;

        // DECIMAL (nullable and non-nullable)
        public static string Safe(this decimal value, string defaultValue = "")
            => value.ToString();

        public static string Safe(this decimal? value, string defaultValue = "")
            => value.HasValue ? value.Value.ToString() : defaultValue;

        // BOOL (nullable only, outputs "Yes"/"No")
        public static string Safe(this bool value, string defaultValue = "")
            => value ? "Yes" : "No";
        public static string Safe(this bool? value, string defaultValue = "")
            => value.HasValue ? (value.Value ? "Yes" : "No") : defaultValue;

        // DATETIME (nullable and non-nullable)
        public static string Safe(this DateTime value, string format = "MM-dd-yyyy", string defaultValue = "")
            => value == DateTime.MinValue ? defaultValue : value.ToString(format);

        public static string Safe(this DateTime? value, string format = "MM-dd-yyyy", string defaultValue = "")
            => value.HasValue && value.Value != DateTime.MinValue
                ? value.Value.ToString(format)
                : defaultValue;

        // DATEONLY (nullable and non-nullable)
        public static string Safe(this DateOnly value, string format = "MM-dd-yyyy", string defaultValue = "")
            => value == DateOnly.MinValue ? defaultValue : value.ToString(format);

        public static string Safe(this DateOnly? value, string format = "MM-dd-yyyy", string defaultValue = "")
            => value.HasValue && value.Value != DateOnly.MinValue
                ? value.Value.ToString(format)
                : defaultValue;

        // TIMEONLY (nullable and non-nullable)
        public static string Safe(this TimeOnly value, string format = "h:mm tt", string defaultValue = "")
            => value == TimeOnly.MinValue ? defaultValue : value.ToString(format);

        public static string Safe(this TimeOnly? value, string format = "h:mm tt", string defaultValue = "")
            => value.HasValue && value.Value != TimeOnly.MinValue
                ? value.Value.ToString(format)
                : defaultValue;
    }

}
