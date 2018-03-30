using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Compilation;
using System.Web.Mvc;
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

                var viewModelVariants = this.LoadVariants(viewModelType);

                return PartialView(context, new MvcPartialComponentViewModel
                {
                    ComponentName = this.Name,
                    PartialName = this.Name,
                    Variants = viewModelVariants
                });
                
            }
            catch (Exception e)
            {
                return PartialView(context, new MvcPartialComponentViewModel
                {
                    ComponentName = this.Name,
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

        private IEnumerable<object> LoadVariants(Type viewModelType)
        {
            var serializer = JsonSerializer.Create(this.serializerSettings);
            
            using (var reader = this.file.OpenText())
            {
                var desc = serializer.Deserialize(reader, typeof(object));
                if (desc is JArray value)
                    return value.Select(i => i.ToObject(viewModelType, serializer)).ToList();
                return Enumerable.Empty<object>();
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