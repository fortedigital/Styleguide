using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Forte.Styleguide.Converters
{
    public class MvcPartialComponentVariantViewModelConverter: JsonConverter
    {
        private readonly Type modelType;

        public MvcPartialComponentVariantViewModelConverter(Type modelType)
        {
            this.modelType = modelType;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            var nameFromJson = jsonObject.SelectToken("name")?.ToObject<string>(serializer);
            var modelFromJson = jsonObject.SelectToken("model")?.ToObject(modelType, serializer);
            var propertiesFromJson = jsonObject.SelectToken("properties")?.ToObject<Dictionary<string, object>>(serializer);
            
            return new MvcPartialComponentVariantViewModel
            {
                Name =  nameFromJson ?? string.Empty,
                Model = modelFromJson,
                Properties = propertiesFromJson ?? new Dictionary<string, object>()
            };
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(MvcPartialComponentVariantViewModel);
        }
    }
}