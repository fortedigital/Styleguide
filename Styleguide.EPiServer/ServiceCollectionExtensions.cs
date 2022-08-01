using System.Collections.Generic;
using System.IO;
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
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Forte.Styleguide.EPiServer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStyleGuideEpiServer(this IServiceCollection services, string featuresRootPath = "Features", string componentFileNameExtension = ".styleguide.json", string layoutPath = null)
        {
            var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IViewCompilerProvider));
            services.Remove(descriptor);
            services.AddSingleton<IViewCompilerProvider, ModuleViewCompilerProvider>();
            services.AddTransient<ComponentCatalogLoader>();
            services.AddTransient<ContentConverter>();
            services.AddTransient<ContentReferenceConverter>();
            services.AddTransient<ContentAreaConverter>();
            services.AddScoped<IViewEngine, RazorViewEngine>();

            services.AddTransient<IStyleguideContentFactory>(provider => new StyleguideContentFactory(
                StyleguideContentEntryPoint.Ensure(provider.GetRequiredService<IContentRepository>()),
                provider.GetRequiredService<IContentTypeRepository>(),
                provider.GetRequiredService<IContentFactory>()));

            services.AddTransient<IStyleguideContentRepository, StyleguideContentRepository>();

            var serviceProvider = services.BuildServiceProvider();
            var iHostEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();
            services.AddTransient<IStyleguideComponentLoader, MvcPartialComponentLoader>(provider =>
                new MvcPartialComponentLoader(Path.Combine(iHostEnvironment.ContentRootPath, featuresRootPath), componentFileNameExtension,
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
