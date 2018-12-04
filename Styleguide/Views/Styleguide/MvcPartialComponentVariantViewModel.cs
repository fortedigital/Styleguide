using System.Collections.Generic;
using Newtonsoft.Json;

namespace Forte.Styleguide
{
    public class MvcPartialComponentVariantViewModel
    {
        public string Name { get; set; }
        public object Model { get; set; }
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public object GetProperty(string name)
        {
            return Properties.TryGetValue(name, out var propertyValue) ? propertyValue : null;
        }
    }
}