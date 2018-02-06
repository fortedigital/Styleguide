using System;
using Newtonsoft.Json;

namespace Styleguide
{
    public class ComponentViewModelDeserializer
    {
        public ComponentViewModelDeserializer()
        {
        }

        public object Deserialize(string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type);
        }
    }
}