using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Forte.Styleguide.EPiServer
{

    public static class ServiceColletionExtensions
    {
        public static IServiceCollection AddStyleGuidee(this IServiceCollection services)
        {
            services.AddControllersWithViews();
            ServiceDescriptor descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IViewCompilerProvider));
            services.Remove(descriptor);
            services.AddSingleton<IViewCompilerProvider, ModuleViewCompilerProvider>();
            services.AddScoped(typeof(ComponentCatalogLoader));

            StyleguideServiceColletionExtensions.AddStyleGuide(services);
            return services;
        }

        public static void UseEpiServerStyleguidee(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            AppDomain.CurrentDomain.SetData("ContentRootPath", env.ContentRootPath);
            AppDomain.CurrentDomain.SetData("WebRootPath", env.WebRootPath);
            StyleguideServiceColletionExtensions.UseEpiServerStyleguide(app, env);
        }
       
    }

}