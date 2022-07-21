using System;
using System.Collections.Generic;
using System.IO;
using EPiServer;
using EPiServer.Construction;
using EPiServer.DataAbstraction;
using Forte.Styleguide.EPiServer.ContentProvider;
using Forte.Styleguide.EPiServer.JsonConverters;
using Newtonsoft.Json;
using StructureMap;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Forte.Styleguide.EPiServer
{

    public static class MyServer
    {
        public static string MapPath(string path)
        {
            return Path.Combine(
                (string)AppDomain.CurrentDomain.GetData("ContentRootPath"),
                path);
        }
    }


    public static class ContainerExtensions
    {
        public static void ConfigureStyleguide(this IContainer container, string featuresRootPath = "~/Features", string componentFileNameExtension = ".styleguide.json", string layoutPath = null)
        {
            container.Configure(config =>
            {
                config.For<IStyleguideContentFactory>().Add(c => new StyleguideContentFactory(
                    StyleguideContentEntryPoint.Ensure(c.GetInstance<IContentRepository>()),
                    c.GetInstance<IContentTypeRepository>(),
                    c.GetInstance<IContentFactory>()));
                
                config.For<IStyleguideContentRepository>().Add<StyleguideContentRepository>();
                
                config.For<IStyleguideComponentLoader>().Add(c => new MvcPartialComponentLoader( MyServer.MapPath(featuresRootPath), componentFileNameExtension, new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter>
                    {
                        c.GetInstance<ContentConverter>(),
                        c.GetInstance<ContentReferenceConverter>(),
                        c.GetInstance<ContentAreaConverter>()
                    }
                }, c.GetInstance<IViewEngine>() ,layoutPath));
                
            });
        }
    }
}
