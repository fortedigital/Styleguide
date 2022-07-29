using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Forte.Styleguide.Views.Styleguide;

namespace Forte.Styleguide
{
    public static class ViewModelDeserializer
    {
        private const string ModelPropertyName = "model";
        private const string VariantsPropertyName = "variants";
        private const string NamePropertyName = "name";
        private const string ViewDataPropertyName = "viewData";

        public static MvcPartialComponentViewModel Deserialize(Type viewModelType, string jsonContent, string name, JsonSerializerSettings serializerSettings = null)
        {
            if (serializerSettings == null) serializerSettings = new JsonSerializerSettings();

            var serializer = JsonSerializer.Create(serializerSettings);

            var viewModelBuilder = new MvcPartialComponentViewModelBuilder()
                .WithName(name);

            var desc = JsonConvert.DeserializeObject(jsonContent, serializerSettings);
            if (desc is JArray value)
            {
                var variants = value.Select(i => i.ToObject(viewModelType, serializer));

                foreach (var variant in variants)
                {
                    viewModelBuilder = viewModelBuilder.WithVariant(builder => builder.WithModel(variant));
                }
            }

            if (desc is JObject jObject)
            {
                var rootModelJsonObject = jObject.SelectToken(ModelPropertyName) ?? new JObject();
                var rootModel = rootModelJsonObject?.ToObject(viewModelType, serializer);

                var variantsToken = jObject.SelectToken(VariantsPropertyName)
                                    ?? throw new InvalidOperationException($"Property '{VariantsPropertyName}' was not found.");

                var jsonSerializer = new JsonSerializer();
                jsonSerializer.Converters.Add(new ResultConverter());

                var variantNo = 1;
                var variantsList = new List<MvcPartialComponentVariantViewModel>();
                foreach (var variant in variantsToken)
                {
                    var rootModelClone = rootModelJsonObject.DeepClone();
                    
                    var variantName = variant.SelectToken(NamePropertyName);
                    var variantModel = variant.SelectToken(ModelPropertyName);
                    var variantViewData = variant.SelectToken(ViewDataPropertyName);

                    var viewModel = new MvcPartialComponentVariantViewModel
                    {
                        Name = variantName?.ToString() ?? (variantsToken.Count() == 1 ? "Normal" : $"Variant {variantNo}"),
                        ViewData = variantViewData?.ToObject<ViewDataDictionary>(jsonSerializer) ?? new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                    };

                    if (rootModelClone is JContainer container)
                    {
                        container.Merge(variantModel, new JsonMergeSettings{ MergeNullValueHandling = MergeNullValueHandling.Merge});
                        viewModel.Model = container.ToObject(viewModelType, serializer);
                    }

                    if (rootModelClone is JValue)
                    {
                        viewModel.Model = variantModel != null ? variantModel.ToObject(viewModelType) : rootModel;
                    }

                    variantsList.Add(viewModel);
                    variantNo++;                    
                }

                if (variantsList.Count == 0)
                {
                    throw new InvalidOperationException("At least one variant should be defined.");
                }

                var partialName = jObject.SelectToken("partialName") != null
                    ? jObject.SelectToken("partialName").ToObject<string>(serializer)
                    : name;

                viewModelBuilder
                    .WithLayout(jObject.SelectToken("layoutPath")?.ToObject<string>(serializer))
                    .WithPartialName(partialName)
                    .WithModel(rootModel)
                    .WithVariants(variantsList.ToArray());
            }

            return viewModelBuilder.Build();
        }
    }
}
