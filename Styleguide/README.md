# Forte.Styleguide

This package allows you to maintain styleguide for your site. It generates page out of your views/partials etc which makes is easy to review styling of all your components (in different variants)

![screenshot](https://user-images.githubusercontent.com/1555694/50396182-99cf3e80-0768-11e9-9bd9-72dff9affffb.jpg)

## Instalation

It is available as [nuget package](https://www.nuget.org/packages/Forte.Styleguide/)

### Configuration

Once installed you need to setup few things:

#### Routing

You need to configure your routing so styleguide is available at certain endpoint. In ASP.NET MVC it will look like this:

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
    
            //remember to put it before "Default" route
            routes.MapRoute("styleguide", "styleguide/{action}/{name}", new {controller = "Styleguide", action = "Index", name = UrlParameter.Optional});
            
            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new {controller = "Home", action = "Index", id = UrlParameter.Optional}
            );
        }

#### Dependency Injection Container

By default `StyleGuideController` has dependency to `ComponentCatalogLoader` so you need to instruct your Dependency Injection Container how to resolve this dependency. 

It will look something like this on example of AutoFac & ASP.NET MVC

    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // ...
            var builder = new ContainerBuilder();

            //you need to tell AutoFac that StyleguideController is something it should be resolving
            builder.RegisterType<StyleguideController>().As<StyleguideController>().InstancePerRequest();
            
            //you may setup MvcPartialComponentLoader with any values you want. Here are example ones
            builder.Register(c => new MvcPartialComponentLoader(
                               HttpContext.Current.Server.MapPath("~/Features"), // ~/Features is a base folder for all your components
                               ".styleguide.json", //is a suffix to files used to propagate data
                               new JsonSerializerSettings()))
                           .As<IStyleguideComponentLoader>();

            builder.RegisterType<ComponentCatalogLoader>();
                
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            
            //...
        }
    }
  
#### Razor (optional)

Since `StyleGuide` is rendering pages with `@Html.Partial` Razor needs to know where to look for your partials. In current example we're using `~/Feautures`, by default Razor will not search for partials there. So you need to do something like this:

  
Create a class extending `RazorViewEngine`
  
      public class FeatureViewLocationRazorViewEngine : RazorViewEngine
      {
          public FeatureViewLocationRazorViewEngine()
          {
              ViewLocationFormats = new[]
              {
                  "~/Features/Pages/{1}/{0}.cshtml",
                  "~/Features/Pages/{1}/{1}.cshtml",
                  "~/Features/{1}/{0}.cshtml"
              };
  
              MasterLocationFormats = new[]
              {
                  "~/Features/Layouts/{0}.cshtml",
              };
              
              PartialViewLocationFormats = new[]
              {
                  "~/Features/{0}.cshtml",                // {0} -> DisplayTemplates\Image
                  "~/Features/{1}/{0}.cshtml",            // {1} -> 
                  "~/Features/Blocks/{1}/{0}.cshtml",
                  "~/Features/Blocks/{0}/Index.cshtml",
                  "~/Features/Partials/{0}.cshtml",
                  "~/Features/Partials/{0}/{0}.cshtml",
                  "~/Features/Partials/{1}/{0}.cshtml",
                  "~/Features/Partials/GenericPage/{0}.cshtml",
                  "~/Features/Partials/GenericBlock/{0}.cshtml",
                  "~/Features/Partials/ContentListBlock/{0}.cshtml",
                  "~/Features/Partials/ObjectListBlock/{0}.cshtml",
                  "~/Features/Partials/SystemBlock/{0}.cshtml",
              };
          }
      }
     
And register it 


    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // ...
            ViewEngines.Engines.Add(new FeatureViewLocationRazorViewEngine());
            //...
        }
    }

### Usage

Once `Styleguide` is configured properly you can create your partial views. Example folder structure will look like

    project
    └───Features
        └───Partials
            └───Test
                  Test.cshtml
                  TestViewModel.cs
                  Test.styleguide.json

`Test.cshtml`:

    @model GenericWebApp.Features.Partials.Test.TestViewModel

    <p>@Model.Name</p>
    @if (Model.ShowTitle)
    {
        <p>@Model.Title</p>
    }


`TestViewModel.cs`:

    namespace GenericWebApp.Features.Partials.Test
    {
        public class TestViewModel
        {
            public string Name { get; set; }
            public string Title { get; set; }
            public bool ShowTitle { get; set; }
        }
    }

`Test.styleguide.json`:

    {
      // it is serialized TestViewModel which will be used as a base for different view variants
      "model" : {
        "name"  : "John",
        "title" : "CEO",
        "showTitle" : true
      },
      // you can use this entry to create different variants of your views.
      "variants":[
        {
          "name" : "With title"
        },
        {
          "name" : "Without title",
          // you can set properties which will be overriden in base view model (set in "main" model entry)
          "model" : {
            "showTitle": false
          }
        }
      ]
    }




