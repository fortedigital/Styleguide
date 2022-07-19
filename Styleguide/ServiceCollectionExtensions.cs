using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Forte.Styleguide
{
    public static class StyleguideServiceColletionExtensions
    {
        public static IServiceCollection AddStyleGuide(this IServiceCollection services)
        {
            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.ViewLocationFormats.Add("Views/{1}/{0}" + RazorViewEngine.ViewExtension);
            });
            services.AddMvc(options => options.EnableEndpointRouting = false);
            return services;
        }

        public static void UseEpiServerStyleguide(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMvc(routes =>
                routes.MapRoute(
                    name: "styleguide",
                    template: "styleguide/{action}",
                    defaults: new { controller = "StyleGuide", action = "Indexxx" }));
        }

    }
}
