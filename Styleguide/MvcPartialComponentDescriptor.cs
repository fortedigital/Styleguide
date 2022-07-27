using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using Styleguide.Views.Styleguide;

namespace Forte.Styleguide
{
    public class MvcPartialComponentDescriptor : IStyleguideComponentDescriptor
    {
        public string Name { get; }
        public string Category { get; }
        public FileInfo File { get; }
        public string LayoutPath { get; }

        private readonly JsonSerializerSettings _serializerSettings;
        private readonly IViewEngine _engine;

        public MvcPartialComponentDescriptor(string name, string category, string layoutPath, FileInfo file, JsonSerializerSettings serializerSettings, IViewEngine engine)
        {
            Name = name;
            Category = category;
            File = file;
            LayoutPath = layoutPath;
            _engine = engine;
            _serializerSettings = serializerSettings;
        }

        public async Task<ActionResult> Execute(ControllerContext context)
        {
            var view = FindPartialView(context, Name);
            if (view == null)
                return new NotFoundObjectResult($"Cound not find partial view {Name}");

            var viewModelType = ResolveViewModelType(view);

            using var reader = this.File.OpenText();
            var jsonContent = await reader.ReadToEndAsync();
            var viewModel = ViewModelDeserializer.Deserialize(viewModelType, jsonContent, Name, _serializerSettings);
            if (string.IsNullOrEmpty(viewModel.LayoutPath))
            {
                viewModel.LayoutPath = this.LayoutPath;
            }
                
            return PartialView(context, viewModel);
        }

        private static PartialViewResult PartialView(ControllerContext context, MvcPartialComponentViewModel model)
        {
            return new PartialViewResult()
            {
                ViewName = "MvcPartialComponent",
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) {{"model", model}}
            };
        }

        private static Type ResolveViewModelType(RazorView view)
        {

            var viewType = ModuleViewCompiler.Current.CompileAsync(view.Path).Result.Type;
            var webViewPageType = viewType
                .GetBaseTypes()
                .FirstOrDefault(t =>t.IsGenericType && t.GetGenericTypeDefinition() == typeof(RazorPage<>));
            
            if (webViewPageType != null)
            {
                return webViewPageType.GetGenericArguments()[0];
            }

            return typeof(object);
        }

        private RazorView FindPartialView(ControllerContext context, string name)
        {
            var result = _engine.FindView(context, name, false);

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
