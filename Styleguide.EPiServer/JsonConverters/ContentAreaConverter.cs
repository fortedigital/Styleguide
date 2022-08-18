using System;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using Forte.Styleguide.EPiServer.ContentProvider;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Forte.Styleguide.EPiServer.JsonConverters
{
    public class ContentAreaConverter : JsonConverter
    {
        private readonly IStyleguideContentFactory factory;
        private readonly IStyleguideContentRepository repository;

        public ContentAreaConverter(IStyleguideContentFactory factory, IStyleguideContentRepository repository)
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
            return objectType == typeof(ContentArea);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            StyleguideContentProvider.EnsureInstalled();
            
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var arrayElement = JArray.Load(reader);
            if (arrayElement == null)
            {
                return null;
            }

            var area = new ContentArea();

            foreach (var element in arrayElement.Children<JObject>())
            {
                var content = this.factory.CreateContent(serializer, element);
                this.repository.AddOrReplace(content);

                area.Items.Add(new ContentAreaItem
                {
                    ContentLink = content.ContentLink, 
                    ContentGuid = content.ContentGuid,
                });
            }

            return area;
        }
    }

    public static class MockContentFactoryExtensions
    {
        public static IContent CreateContent(this IStyleguideContentFactory factory, JsonSerializer serializer, JObject element, Type modelType = null)
        {
            var id = element.GetId();
            var name = element.GetString("Name");
            var contentTypeName = element.GetString("ContentType");
            var properties = element.GetProperties();

            IContent content;
            if (string.IsNullOrEmpty(contentTypeName))                
                content = factory.CreateContent(id, name, modelType ?? typeof(GenericStyleguideContent), properties);                
            else
                content = factory.CreateContent(id, name, contentTypeName, properties);
            
            serializer.Populate(element.CreateReader(), content);

            return content;
        }
    }
    
    [ContentType(GUID = "CC2E3E6A-2F52-4000-9B48-E2FF0D4A6E8C", AvailableInEditMode = false)]
    public class GenericStyleguideContent : PageData
    {
    }    
}