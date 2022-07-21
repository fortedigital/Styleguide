using System.Collections.Generic;

namespace Forte.Styleguide
{
    public interface IStyleguideComponentLoader
    {
        IEnumerable<IStyleguideComponentDescriptor> LoadComponents();
    }
}
