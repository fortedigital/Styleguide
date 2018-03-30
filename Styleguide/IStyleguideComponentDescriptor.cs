using System.Web.Mvc;

namespace Forte.Styleguide
{
    public interface IStyleguideComponentDescriptor
    {
        string Name { get; }
        string Category { get; }
        ActionResult Execute(ControllerContext context);
    }
}