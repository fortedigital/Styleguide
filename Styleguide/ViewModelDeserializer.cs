using System;
using System.Collections.Generic;
using System.Linq;
using Forte.Styleguide.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Forte.Styleguide
{
    public static class ViewModelDeserializer
    {
        public static MvcPartialComponentViewModel Deserialize(Type viewModelType, string jsonContent, string name, JsonSerializerSettings serializerSettings = null)
        {
            if (serializerSettings == null) serializerSettings = new JsonSerializerSettings();
            
            var serializer = JsonSerializer.Create(serializerSettings);
            
            var viewModelBuilder = new MvcPartialComponentViewModelBuilder()
                .WithName(name)
                .WithPartialName(name);

            var desc = JsonConvert.DeserializeObject(jsonContent, serializerSettings);            
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
                
                var variantsList = new List<MvcPartialComponentVariantViewModel>();
                foreach (var variant in variantsToken)
                {
                    var rootModelCopy = rootModelJsonObject.DeepClone();
                    if (!(rootModelCopy is JContainer container)) continue;
                    
                    var variantModel = variant.SelectToken("model");
                    container.Merge(variantModel);
                    variantsList.Add(new MvcPartialComponentVariantViewModel
                    {
                        Model = container.ToObject(viewModelType, serializer),
                        Name = variant.SelectToken("name").ToString()
                    });
                }
                
                viewModelBuilder = viewModelBuilder
                    .WithPartialName(jObject.SelectToken("layout")?.ToObject<string>(serializer))
                    .WithModel(rootModel)
                    .WithVariants(variantsList.ToArray());
            }

            return viewModelBuilder.Build();
        }
    }
}
