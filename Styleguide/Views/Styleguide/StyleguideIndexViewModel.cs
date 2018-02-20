using System.Collections.Generic;

namespace Forte.Styleguide
{
    public class StyleguideIndexViewModel
    {
        public IEnumerable<IStyleguideComponentDescriptor> Components { get; }

        public StyleguideIndexViewModel(IEnumerable<IStyleguideComponentDescriptor> components)
        {
            Components = components;
        }
    }
}