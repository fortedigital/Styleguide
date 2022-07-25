using Microsoft.AspNetCore.Mvc;

namespace Styleguide
{
    public interface IStyleguideComponentDescriptor
    {
        string Name { get; }
        string Category { get; }
        FileInfo File { get; }
        Task<ActionResult> Execute(ControllerContext context);
    }
}
