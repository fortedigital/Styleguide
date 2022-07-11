using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.Logging;

namespace Forte.Styleguide
{
    public class ModuleViewCompilerProvider : IViewCompilerProvider
    {
        protected IViewCompiler Compiler { get; }

        public ModuleViewCompilerProvider(ApplicationPartManager applicationPartManager, ILoggerFactory loggerFactory)
        {
            Compiler = new ModuleViewCompiler(applicationPartManager, loggerFactory);
        }

        public IViewCompiler GetCompiler()
        {
            return Compiler;
        }

    }
}
