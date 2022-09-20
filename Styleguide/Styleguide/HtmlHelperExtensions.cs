using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Forte.Styleguide
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString StyleguideComponentLink(this HtmlHelper html, IStyleguideComponentDescriptor component,
            object htmlAttributes = null)
        {
            return html.ActionLink(
                component.Name, 
                "Component", 
                "Styleguide", 
                new { name = component.Name },
                htmlAttributes ?? new object());
        }
    }
}