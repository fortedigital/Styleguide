using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Forte.Styleguide
{
    public static class InitialDataDeserializer
    {
        private const string DisplayNamePropertyName = "displayName";
        private const string TagsPropertyName = "tags";
        
        public static InitialData Deserialize(string jsonContent, JsonSerializerSettings serializerSettings)
        {
            var serializer = JsonSerializer.Create(serializerSettings);
            var desc = JsonConvert.DeserializeObject(jsonContent, serializerSettings);

            if (desc is JObject jObject)
            {
                var displayName = jObject.SelectToken(DisplayNamePropertyName)?.ToObject<string>(serializer);
                var tags = jObject.SelectToken(TagsPropertyName)?.ToObject<List<string>>(serializer);

                return new InitialData(displayName, tags);
            }

            return new InitialData();
        }
    }
}