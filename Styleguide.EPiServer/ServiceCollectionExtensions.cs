using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Construction;
using EPiServer.DataAbstraction;
using Forte.Styleguide.EPiServer.ContentProvider;
using Forte.Styleguide.EPiServer.JsonConverters;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Forte.Styleguide.EPiServer
{

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStyleGuideEpiServer(this IServiceCollection services, string featuresRootPath = "Features", string componentFileNameExtension = ".styleguide.json", string layoutPath = null)
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
            services.AddTransient(typeof(ComponentCatalogLoader));
            services.AddTransient(typeof(ContentConverter));
            services.AddTransient(typeof(ContentReferenceConverter));
            services.AddTransient(typeof(ContentAreaConverter));
            services.AddScoped<IViewEngine, RazorViewEngine>();

            services.AddTransient<IStyleguideContentFactory>(provider => new StyleguideContentFactory(
                StyleguideContentEntryPoint.Ensure(provider.GetRequiredService<IContentRepository>()),
                provider.GetRequiredService<IContentTypeRepository>(),
                provider.GetRequiredService<IContentFactory>()));

            services.AddTransient<IStyleguideContentRepository, StyleguideContentRepository>();


            services.AddTransient<IStyleguideComponentLoader, MvcPartialComponentLoader>(provider =>
                new MvcPartialComponentLoader(featuresRootPath, componentFileNameExtension,
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

    }

}
