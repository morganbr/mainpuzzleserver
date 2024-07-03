using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ServerCore.Helpers
{
    public static class RawHtmlHelper
    {
        /// <summary>
        /// Helper for extracting raw HTML for Razor Pages from a string if present.
        /// The syntax we look for is: {anything}Html.Raw({raw html})
        /// If we see that syntax, we return the {raw html} part only.
        /// If we see anything else, we return the entire string, processed by the standard ASP.NET IHtmlHelper.
        /// </summary>
        /// <param name="text">text which might contain raw HTML in</param>
        /// <returns>IHtmlContent containing either the raw html processed by Html.Raw or the entire string processed by Html.DisplayFor</returns>
        public static IHtmlContent Display<T>(string text, int eventId, IHtmlHelper<T> helper)
        {
            // Note that because this returns an ASP object, this can't be used in Javascript on pages that have already loaded
            // For example, Pages\Submissions\Index.cshtml duplicates this stripping of "Html.Raw(" in Javascript since it can't call this
            string asRawHtml = GetRawHtml(text, eventId);
            if (asRawHtml != null)
            {
                return helper.Raw(asRawHtml);
            }
            else
            {
                return helper.DisplayFor(m => text);
            }
        }

        /// <summary>
        /// Helper for extracting raw HTML for Blazor from a string if present.
        /// The syntax we look for is: {anything}Html.Raw({raw html})
        /// If we see that syntax, we return the {raw html} part only.
        /// If we see anything else, we return the entire string, processed by the standard ASP.NET IHtmlHelper.
        /// </summary>
        /// <param name="text">text which might contain raw HTML in</param>
        /// <returns>MarkupString containing the raw html processed by Html.Raw or the entire string</returns>

        public static object GetMarkupString(string text, int eventId)
        {
            string asRawHtml = GetRawHtml(text, eventId);
            if (asRawHtml != null)
            {
                return new MarkupString(asRawHtml);
            }
            else
            {
                return text;
            }
        }

        private static string? GetRawHtml(string text, int eventId) 
        {
            if (text != null && text.EndsWith(")") && text.Contains("Html.Raw("))
            {
                text = text.Replace("{eventId}", $"{eventId}");
                int start = text.IndexOf("Html.Raw(") + 9;
                return text.Substring(start, text.Length - start - 1);
            }

            return null;
        }
    }
}
