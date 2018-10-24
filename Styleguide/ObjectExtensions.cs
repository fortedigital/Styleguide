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
            if (objectType != content.GetType())
            {
                throw new Exception("");
            }

            var jObject = JObject.FromObject(@object, serializer);
            var contentJObject = JObject.FromObject(content);
            
            jObject.Merge(contentJObject);

            return jObject.ToObject(objectType, serializer);
        }
    }
}