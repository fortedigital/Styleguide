using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Compilation;
using System.Web.Mvc;
using Forte.Styleguide.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Forte.Styleguide
{
    public class MvcPartialComponentDescriptor : IStyleguideComponentDescriptor
    {
        public string Name { get; }
        public string Category { get; }

        private readonly FileInfo file;
        private readonly JsonSerializerSettings serializerSettings;

        public MvcPartialComponentDescriptor(string name, string category, FileInfo file, JsonSerializerSettings serializerSettings)
        {
            this.Name = name;
            this.Category = category;
            this.file = file;
            this.serializerSettings = serializerSettings;
        }

        public ActionResult Execute(ControllerContext context)
        {
            var view = this.FindPartialView(context, this.Name);
            if (view == null)
                return new HttpNotFoundResult($"Cound not find partial view {this.Name}");

            try
            {
                var viewModelType = this.ResolveViewModelType(view);

                var viewModel = this.LoadComponentViewModel(viewModelType);

                return PartialView(context, viewModel);
                
            }
            catch (Exception e)
            {
                return PartialView(context, new MvcPartialComponentViewModel
                {
                    Name = this.Name,
                    Error = e.ToString()
                });
            }
        }

        private static PartialViewResult PartialView(ControllerContext context, MvcPartialComponentViewModel model)
        {
            return new PartialViewResult()
            {
                // ReSharper disable once Mvc.ViewNotResolved
                View = ViewEngines.Engines.FindView(context, "MvcPartialComponent", null).View,
                ViewName = "MvcPartialComponent",
                ViewData = new ViewDataDictionary(model),
                ViewEngineCollection = ViewEngines.Engines
            };
        }

        private MvcPartialComponentViewModel LoadComponentViewModel(Type viewModelType)
        {
            var serializer = JsonSerializer.Create(this.serializerSettings);
            
            serializer.Converters.Add(new MvcPartialComponentVariantViewModelConverter(viewModelType));

            var viewModelBuilder = new MvcPartialComponentViewModelBuilder()
                .WithName(this.Name)
                .WithPartialName(this.Name);
            
            using (var reader = this.file.OpenText())
            {
                var desc = serializer.Deserialize(reader, typeof(object));
                if (desc is JArray value)
                {
                    var variants = value.Select(i => i.ToObject(viewModelType, serializer));

                    foreach (var variant in variants)
                    {
                        viewModelBuilder = viewModelBuilder.WithVariant(builder => builder.WithModel(variant));
                    }
                }
                
                if(desc is JObject jObject)
                {
                    viewModelBuilder = viewModelBuilder
                        .WithPartialName(jObject.SelectToken("layout")?.ToObject<string>(serializer))
                        .WithModel(jObject.SelectToken("model")?.ToObject(viewModelType, serializer))
                        .WithVariants(jObject.SelectToken("variants")?.ToObject<MvcPartialComponentVariantViewModel[]>(serializer));
                }

                return viewModelBuilder.Build();
            }
        }

        private Type ResolveViewModelType(BuildManagerCompiledView view)
        {
            var viewType = BuildManager.GetCompiledType(view.ViewPath);
            var webViewPageType = viewType
                .GetBaseTypes()
                .FirstOrDefault(t =>t.IsGenericType && t.GetGenericTypeDefinition() == typeof(WebViewPage<>));
            
            if (webViewPageType != null)
            {
                return webViewPageType.GetGenericArguments()[0];
            }

            return typeof(object);
        }

        private RazorView FindPartialView(ControllerContext context, string name)
        {
            var result = ViewEngines.Engines.FindPartialView(context, this.Name);

            return result?.View as RazorView;
        }
    }

    public static class TypeExtensions
    {
        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            for (var t = type; t != null; t = t.BaseType)
            {
                yield return t;
            }
        }
    }
}