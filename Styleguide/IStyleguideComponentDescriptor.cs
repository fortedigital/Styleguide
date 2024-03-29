﻿using Microsoft.AspNetCore.Mvc;

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
