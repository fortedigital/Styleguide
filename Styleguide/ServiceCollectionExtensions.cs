﻿using System;
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
    public static class StyleguideServiceCollectionExtensions
    {
        public static IServiceCollection AddStyleGuideEpiserverCore(this IServiceCollection services)
        {
            services.AddMvc(options => options.EnableEndpointRouting = false);
            return services;
        }

        public static void UseStyleguideEpiserverCore(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMvc(routes =>
                routes.MapRoute(
                    name: "styleguide",
                    template: "styleguide/{action}",
                    defaults: new { controller = "StyleGuide", action = "Indexxx" }));
        }

    }
}
