using System.Collections.Generic;

namespace Forte.Styleguide
{
    public class StyleguideIndexViewModel
    {
        public IEnumerable<IStyleguideComponentDescriptor> Components { get; }
        public IEnumerable<string> Tags { get; }

        public StyleguideIndexViewModel(
            IEnumerable<IStyleguideComponentDescriptor> components, 
            IEnumerable<string> tags)
        {
            Components = components;
            Tags = tags;
        }
    }
}