namespace Styleguide
{
    public interface IStyleguideComponentLoader
    {
        IEnumerable<IStyleguideComponentDescriptor> LoadComponents();
    }
}
