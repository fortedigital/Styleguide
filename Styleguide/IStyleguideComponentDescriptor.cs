using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Forte.Styleguide
{
    public interface IStyleguideComponentDescriptor
    {
        string Name { get; }
        string DisplayName { get; }
        string Category { get; }
        FileInfo File { get; }
        Task<ActionResult> Execute(ControllerContext context);
    }
}
