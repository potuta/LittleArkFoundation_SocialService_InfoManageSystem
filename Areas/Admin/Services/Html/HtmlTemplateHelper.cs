using HtmlAgilityPack;

namespace LittleArkFoundation.Areas.Admin.Services.Html
{
    public static class HtmlTemplateHelper
    {
        /// <summary>
        /// Sets the background-color of a checkbox div if the condition is true.
        /// </summary>
        public static void SetCheckboxStyle(HtmlDocument htmlDoc, string className, bool isChecked)
        {
            if (!isChecked) return;

            var node = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='{className}']");
            if (node != null)
            {
                string existingStyle = node.GetAttributeValue("style", "");
                node.SetAttributeValue("style", $"{existingStyle}; background-color: black;");
            }
        }

        /// <summary>
        /// Sets inner HTML safely, replacing null or empty with a default.
        /// </summary>
        public static void SetInnerHtml(this HtmlDocument htmlDoc, string xpath, string content)
        {
            var node = htmlDoc.DocumentNode.SelectSingleNode(xpath);
            if (node != null)
                node.InnerHtml = content.Safe();
        }
    }
}
