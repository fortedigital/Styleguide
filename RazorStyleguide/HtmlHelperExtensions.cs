
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Forte.Styleguide
{
    public static class HtmlHelperExtensions
    {
        public static HtmlString StyleguideComponentLink(this IHtmlHelper html, IStyleguideComponentDescriptor component,
            object htmlAttributes = null)
        {
            return html.ActionLink(
                component.Name, 
                "Component", 
                "Styleguide", 
                new { name = component.Name },
                htmlAttributes ?? new object()) as HtmlString;
        }
    }
}