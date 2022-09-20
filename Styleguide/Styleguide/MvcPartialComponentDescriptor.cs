using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Compilation;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace Forte.Styleguide
{
    public class MvcPartialComponentDescriptor : IStyleguideComponentDescriptor
    {
        public string Name { get; }
        public string Category { get; private set; }
        public FileInfo File { get; }
        public string LayoutPath { get; }

        private readonly JsonSerializerSettings serializerSettings;

        public MvcPartialComponentDescriptor(string name, string category, string layoutPath, FileInfo file, JsonSerializerSettings serializerSettings)
        {
            this.Name = name;
            this.Category = category;
            this.File = file;
            this.LayoutPath = layoutPath;
            this.serializerSettings = serializerSettings;
        }

        public async Task<ActionResult> Execute(ControllerContext context)
        {
            var view = FindPartialView(context, this.Name);
            if (view == null)
                return new HttpNotFoundResult($"Cound not find partial view {this.Name}");

            var viewModelType = ResolveViewModelType(view);

            using (var reader = this.File.OpenText())
            {
                var jsonContent = await reader.ReadToEndAsync();
                var viewModel = ViewModelDeserializer.Deserialize(viewModelType, jsonContent, this.Name, this.serializerSettings);
                if (string.IsNullOrEmpty(viewModel.LayoutPath))
                {
                    viewModel.LayoutPath = this.LayoutPath;
                }

                Category = viewModel.Category ?? Category;
                
                return PartialView(context, viewModel);
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

        private static Type ResolveViewModelType(BuildManagerCompiledView view)
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

        private static RazorView FindPartialView(ControllerContext context, string name)
        {
            var result = ViewEngines.Engines.FindPartialView(context, name);

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
