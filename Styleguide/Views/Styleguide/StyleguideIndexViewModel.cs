namespace Forte.Styleguide.Views.Styleguide
{
    public class StyleguideIndexViewModel
    {
        public IEnumerable<IStyleguideComponentDescriptor> Components { get; }
        public IEnumerable<string> Tags { get; }
        public bool UseMarkdownDescription { get; }

        public StyleguideIndexViewModel(
            IEnumerable<IStyleguideComponentDescriptor> components, 
            IEnumerable<string> tags,
            bool useMarkdownDescription)
        {
            Components = components;
            Tags = tags;
            UseMarkdownDescription = useMarkdownDescription;
        }
    }
}
