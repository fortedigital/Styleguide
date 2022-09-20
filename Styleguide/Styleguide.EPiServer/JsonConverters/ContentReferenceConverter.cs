using System;
using EPiServer.Core;
using Forte.Styleguide.EPiServer.ContentProvider;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Forte.Styleguide.EPiServer.JsonConverters
{
    public class ContentReferenceConverter : JsonConverter
    {
        private readonly IStyleguideContentFactory factory;
        private readonly IStyleguideContentRepository repository;

        public ContentReferenceConverter(IStyleguideContentFactory factory, IStyleguideContentRepository repository)
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
            return objectType == typeof(ContentReference);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            StyleguideContentProvider.EnsureInstalled();
            
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var element = JObject.Load(reader);
            var content = this.factory.CreateContent(serializer, element);

            this.repository.AddOrReplace(content);

            return content.ContentLink;
        }
    }
}