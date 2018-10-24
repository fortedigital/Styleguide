using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Forte.Styleguide
{
    public static class ObjectExtensions
    {
        public static object Merge(this object @object, object content, JsonSerializer serializer)
        {
            var objectType = @object.GetType();
            var contentType = content.GetType();
            
            if (objectType != contentType)
            {
                throw new Exception($"Expected type of content to be merged: {objectType} but was {contentType}.");
            }

            var jObject = JObject.FromObject(@object, serializer);
            var contentJObject = JObject.FromObject(content);
            
            jObject.Merge(contentJObject);

            return jObject.ToObject(objectType, serializer);
        }
    }
}