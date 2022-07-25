using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Construction;
using EPiServer.DataAbstraction;
using Forte.Styleguide.EPiServer.ContentProvider;
using Forte.Styleguide.EPiServer.JsonConverters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Styleguide;

namespace Forte.Styleguide.EPiServer
{

    public static class MyServer
    {
        public static string MapPath(string path)
        {
            return path;
            //return Path.Combine(
            //    (string)AppDomain.CurrentDomain.GetData("ContentRootPath"),
            //    path);
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStyleGuideEPiServer(this IServiceCollection services, string featuresRootPath = "Features", string componentFileNameExtension = ".styleguide.json", string layoutPath = null)
        {
            services.AddControllersWithViews();
            var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IViewCompilerProvider));
            services.Remove(descriptor);
            services.AddSingleton<IViewCompilerProvider, ModuleViewCompilerProvider>();
            services.AddScoped(typeof(ComponentCatalogLoader));
            services.AddScoped(typeof(ContentConverter));
            services.AddScoped(typeof(ContentReferenceConverter));
            services.AddScoped(typeof(ContentAreaConverter));
            services.AddScoped<IViewEngine, RazorViewEngine>();

            services.AddScoped<IStyleguideContentFactory>(provider => new StyleguideContentFactory(
                StyleguideContentEntryPoint.Ensure(provider.GetRequiredService<IContentRepository>()),
                provider.GetRequiredService<IContentTypeRepository>(),
                provider.GetRequiredService<IContentFactory>()));

            services.AddScoped<IStyleguideContentRepository, StyleguideContentRepository>();


            services.AddScoped<IStyleguideComponentLoader, MvcPartialComponentLoader>(provider =>
                new MvcPartialComponentLoader(MyServer.MapPath(featuresRootPath), componentFileNameExtension,
                    new JsonSerializerSettings
                    {
                        Converters = new List<JsonConverter>()
                        {
                            provider.GetRequiredService<ContentConverter>(),
                            provider.GetRequiredService<ContentReferenceConverter>(),
                            provider.GetRequiredService<ContentAreaConverter>()
                        }
                    },
                    provider.GetRequiredService<IViewEngine>(),
                    layoutPath));

            return services;
        }

        public static void UseStyleguideEPiServer(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            //AppDomain.CurrentDomain.SetData("ContentRootPath", env.ContentRootPath);
            //AppDomain.CurrentDomain.SetData("WebRootPath", env.WebRootPath);
        }
       
    }

}
