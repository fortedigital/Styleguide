using System;
using System.Collections.Generic;
using System.Linq;

namespace Forte.Styleguide
{
    public class ComponentCatalog
    {
        public readonly IReadOnlyCollection<IStyleguideComponentDescriptor> Components;

        public ComponentCatalog(IReadOnlyCollection<IStyleguideComponentDescriptor> components)
        {
            this.Components = components;
        }

        public IStyleguideComponentDescriptor GetComponentByName(string name)
        {
            return Components.SingleOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
