using System.Web.Mvc;

namespace Forte.Styleguide
{
    public interface IStyleguideComponentDescriptor
    {
        string Name { get; }
        ActionResult Execute(ControllerContext context);
    }
}