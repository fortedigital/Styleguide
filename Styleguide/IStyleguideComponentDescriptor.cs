using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Forte.Styleguide
{
    public interface IStyleguideComponentDescriptor
    {
        string Name { get; }
        string Category { get; }
        FileInfo File { get; }
        Task<ActionResult> Execute(ControllerContext context);
    }
}
