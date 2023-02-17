using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Forte.Styleguide
{
    public class InitialDataDeserializer
    {
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly JsonSerializer _jsonSerializer;
        private const string DisplayNamePropertyName = "displayName";
        private const string TagsPropertyName = "tags";

        public InitialDataDeserializer(JsonSerializerSettings serializerSettings)
        {
            _serializerSettings = serializerSettings;
            _jsonSerializer = JsonSerializer.Create(_serializerSettings);
        }

        public InitialData Deserialize(string jsonContent)
        {
            var desc = JsonConvert.DeserializeObject(jsonContent, _serializerSettings);

            if (desc is JObject jObject)
            {
                var displayName = GetValue(jObject, DisplayNamePropertyName, string.Empty);
                var tags = GetValue(jObject, TagsPropertyName, new List<string>());
                
                return new InitialData(displayName, tags);
            }

            return new InitialData();
        }

        private T GetValue<T>(JToken jObject, string path, T defaultValue)
        {
            var token = jObject.SelectToken(path);
            if (token == null)
                return defaultValue;

            return token.ToObject<T>(_jsonSerializer);
        }
    }
}
