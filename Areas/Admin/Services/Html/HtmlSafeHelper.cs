using HtmlAgilityPack;

namespace LittleArkFoundation.Areas.Admin.Services.Html
{
    public static class HtmlSafeHelper
    {
        // STRING
        public static string Safe(this string? value, string defaultValue = "")
            => string.IsNullOrWhiteSpace(value) ? defaultValue : value!;

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
        public static string Safe(this bool? value, string defaultValue = "")
            => value.HasValue ? (value.Value ? "Yes" : "No") : defaultValue;

        // DATETIME (nullable and non-nullable)
        public static string Safe(this DateTime value, string format = "yyyy-MM-dd", string defaultValue = "")
            => value.ToString(format);

        public static string Safe(this DateTime? value, string format = "yyyy-MM-dd", string defaultValue = "")
            => value.HasValue ? value.Value.ToString(format) : defaultValue;

        // DATEONLY (nullable and non-nullable)
        public static string Safe(this DateOnly value, string format = "yyyy-MM-dd", string defaultValue = "")
            => value.ToString(format);

        public static string Safe(this DateOnly? value, string format = "yyyy-MM-dd", string defaultValue = "")
            => value.HasValue ? value.Value.ToString(format) : defaultValue;

        // TIMEONLY (nullable and non-nullable)
        public static string Safe(this TimeOnly value, string format = "HH:mm", string defaultValue = "")
            => value.ToString(format);

        public static string Safe(this TimeOnly? value, string format = "HH:mm", string defaultValue = "")
            => value.HasValue ? value.Value.ToString(format) : defaultValue;
    }

}
