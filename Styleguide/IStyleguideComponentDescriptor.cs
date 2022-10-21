using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Forte.Styleguide
{
    public interface IStyleguideComponentDescriptor
    {
        string Name { get; }
        string DisplayName { get; }
        IEnumerable<string> Tags { get; }
        FileInfo File { get; }
        FileInfo MarkdownFile { get; }
        Task<ActionResult> Execute(ControllerContext context);
    }
}
