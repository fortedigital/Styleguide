namespace Styleguide.Views.Styleguide
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
