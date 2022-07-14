using System.IO;
using System.Threading.Tasks;
#if NETSTANDARD2_0
    using Microsoft.AspNetCore.Mvc;
#else
    using System.Web.Mvc;
#endif

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
