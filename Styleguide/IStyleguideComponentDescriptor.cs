using System.IO;
using System.Web.Mvc;

namespace Forte.Styleguide
{
    public interface IStyleguideComponentDescriptor
    {
        string Name { get; }
        string Category { get; }
        FileInfo File { get; }
        ActionResult Execute(ControllerContext context);
    }
}