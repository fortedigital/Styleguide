using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Forte.Styleguide.EPiServer
{
    public static class ServiceColletionExtensions
    {
        public static IServiceCollection AddStyleGuide(this IServiceCollection services)
        {

            return services;
        }

        public static void UseEpiServerStyleguide(this IApplicationBuilder app)
        {
            app.UseMvc(routes =>
                routes.MapRoute(
                    name: "styleguide",
                    template: "styleguide/{action}/{name?}",
                    defaults: new { controller = "styleguide", action = "index" }));
        }
    }
}