using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.DependencyInjection;

namespace Forte.Styleguide.EPiServer
{

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStyleGuideEpiserver(this IServiceCollection services)
        {
            services.AddControllersWithViews();
            var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IViewCompilerProvider));
            services.Remove(descriptor);
            services.AddSingleton<IViewCompilerProvider, ModuleViewCompilerProvider>();
            services.AddScoped(typeof(ComponentCatalogLoader));
            services.AddStyleGuideEpiserverCore();
            return services;
        }

        public static void UseStyleguideEpiserver(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            AppDomain.CurrentDomain.SetData("ContentRootPath", env.ContentRootPath);
            AppDomain.CurrentDomain.SetData("WebRootPath", env.WebRootPath);
            app.UseStyleguideEpiserverCore(env);
        }
    }

}
