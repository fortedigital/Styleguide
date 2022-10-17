using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using EPiServer;
using EPiServer.Construction;
using EPiServer.DataAbstraction;
using Forte.Styleguide.EPiServer.ContentProvider;
using Forte.Styleguide.EPiServer.JsonConverters;
using Newtonsoft.Json;
using StructureMap;

namespace Forte.Styleguide.EPiServer
{
    public static class ContainerExtensions
    {
        public static void ConfigureStyleguide(this IContainer container, string featuresRootPath = "~/Features", string componentFileNameExtension = ".styleguide.json", string layoutPath = null)
        {
            RouteTable.Routes.MapRoute(
                "styleguide",
                "styleguide/{action}/{name}",
                new { controller = "styleguide", action = "index", name = UrlParameter.Optional }
            );

            container.Configure(config =>
            {
                config.For<IStyleguideContentFactory>().Add(c => new StyleguideContentFactory(
                    StyleguideContentEntryPoint.Ensure(c.GetInstance<IContentRepository>()),
                    c.GetInstance<IContentTypeRepository>(),
                    c.GetInstance<IContentFactory>()));
                
                config.For<IStyleguideContentRepository>().Add<StyleguideContentRepository>();
                
                config.For<IStyleguideComponentLoader>().Add(c => new MvcPartialComponentLoader(HttpContext.Current.Server.MapPath(featuresRootPath), componentFileNameExtension, new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter>
                    {
                        c.GetInstance<ContentConverter>(),
                        c.GetInstance<ContentReferenceConverter>(),
                        c.GetInstance<ContentAreaConverter>()
                    }
                }, layoutPath));
            });
        }
    }
}
