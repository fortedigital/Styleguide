using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Forte.Styleguide
{
    class ResultConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(ViewDataDictionary));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            // Read the properties which will be used as constructor parameters
            var variant = jo.ToObject<Dictionary<string, object>>();

            // Construct the Result object using the non-default constructor
            var result = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());

            foreach (var keyValuePair in variant)
            {
                result.Add(keyValuePair);
            }
            return result;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
