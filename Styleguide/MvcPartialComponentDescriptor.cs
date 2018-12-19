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
        public FileInfo File { get; }
        
        private readonly JsonSerializerSettings serializerSettings;

        public MvcPartialComponentDescriptor(string name, string category, FileInfo file, JsonSerializerSettings serializerSettings)
        {
            this.Name = name;
            this.Category = category;
            this.File = file;
            this.serializerSettings = serializerSettings;
        }

        public ActionResult Execute(ControllerContext context)
        {
            var view = this.FindPartialView(context, this.Name);
            if (view == null)
                return new HttpNotFoundResult($"Cound not find partial view {this.Name}");

            var viewModelType = this.ResolveViewModelType(view);

            var viewModel = this.LoadComponentViewModel(viewModelType);

            return PartialView(context, viewModel);
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
            
            var viewModelBuilder = new MvcPartialComponentViewModelBuilder()
                .WithName(this.Name)
                .WithPartialName(this.Name);
            
            using (var reader = this.File.OpenText())
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
                    serializer.Converters.Add(new MvcPartialComponentVariantViewModelConverter(viewModelType));

                    var rootModelJsonObject = jObject.SelectToken("model") ?? new JObject();
                    var rootModel = rootModelJsonObject?.ToObject(viewModelType, serializer);
                    
                    var variantsToken = jObject.SelectToken("variants"); 
                    var variantsModelTokens = variantsToken?.Select(token => token.SelectToken("model"));
                    
                    foreach (var variantModelToken in variantsModelTokens ?? Enumerable.Empty<JToken>())
                    {
                        var rootModelCopy = rootModelJsonObject.DeepClone();
                        if (rootModelCopy is JContainer container)
                        {
                            container.Merge(variantModelToken);
                            variantModelToken.Replace(rootModelCopy);
                        }
                    }

                    var variantsAfterMerge = variantsToken?.ToObject<MvcPartialComponentVariantViewModel[]>(serializer) ?? new MvcPartialComponentVariantViewModel[0];
                    
                    viewModelBuilder = viewModelBuilder
                        .WithPartialName(jObject.SelectToken("layout")?.ToObject<string>(serializer))
                        .WithModel(rootModel)
                        .WithVariants(variantsAfterMerge);
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