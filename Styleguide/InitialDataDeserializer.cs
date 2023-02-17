using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Forte.Styleguide
{
    public static class InitialDataDeserializer
    {
        private const string DisplayNamePropertyName = "displayName";

        public static InitialData Deserialize(string jsonContent, JsonSerializerSettings serializerSettings)
        {
            var serializer = JsonSerializer.Create(serializerSettings);
            var desc = JsonConvert.DeserializeObject(jsonContent, serializerSettings);

            if (desc is JObject jObject)
            {
                var displayName = jObject.SelectToken(DisplayNamePropertyName)?.ToObject<string>(serializer);

                return new InitialData(displayName);
            }

            return new InitialData();
        }
    }
}
