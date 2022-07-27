using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;
using Forte.Styleguide.EPiServer.ContentProvider;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Forte.Styleguide.EPiServer.JsonConverters
{
    public class ContentConverter : JsonConverter
    {
        private readonly IStyleguideContentFactory factory;
        private readonly IStyleguideContentRepository repository;

        public ContentConverter(IStyleguideContentFactory factory, IStyleguideContentRepository repository)
        {
            this.factory = factory;
            this.repository = repository;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IContent).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            StyleguideContentProvider.EnsureInstalled();
            
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var element = JObject.Load(reader);
            var content = this.factory.CreateContent(serializer, element, objectType);

            this.repository.AddOrReplace(content);

            return content;
        }
    }

    public static class JObjectExtensions
    {
        public static string GetString(this JObject jObject, string propertyName, string fallback = "")
        {
            return jObject.GetValue(propertyName, fallback);
        }

        public static int GetInt(this JObject jObject, string propertyName, int fallback = 0)
        {
            return jObject.GetValue(propertyName, fallback);
        }

        public static T GetValue<T>(this JObject jObject, string propertyName, T fallback = default(T))
        {
            JToken nameToken;

            return jObject.TryGetValue(propertyName, StringComparison.InvariantCultureIgnoreCase, out nameToken)
                ? nameToken.Value<T>()
                : fallback;
        }

        public static int GetId(this JObject jObject)
        {
            return jObject.GetInt("Id", StyleguideContentIdSequence.Next());
        }

        public static Dictionary<string, object> GetProperties(this JObject contentElement)
        {
            var properties = contentElement.Properties().ToList();

            return FilterPropertiesOfType<string>(properties, JTokenType.Boolean)
                .Concat(FilterPropertiesOfType<string>(properties, JTokenType.Integer))
                .Concat(FilterPropertiesOfType<string>(properties, JTokenType.Float))
                .Concat(FilterPropertiesOfType<string>(properties, JTokenType.Date))
                .Concat(FilterPropertiesOfType<string>(properties, JTokenType.String))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);
        }

        private static Dictionary<string, object> FilterPropertiesOfType<T>(IEnumerable<JProperty> properties, JTokenType type)
        {
            return properties
               .Where(x => x.Type == JTokenType.Property)
               .Where(x => x.Value.Type == type)
               .ToDictionary(x => x.Name, x => (object)x.Value.Value<T>());
        }
    }
}
