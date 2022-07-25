namespace Styleguide
{
    public class ComponentCatalogLoader
    {
        private ComponentCatalog _catalog;

        private readonly object _syncLock = new object();        
        private readonly IEnumerable<IStyleguideComponentLoader> _loaders;

        public ComponentCatalogLoader(IEnumerable<IStyleguideComponentLoader> loaders)
        {
            _loaders = loaders;
        }

        public ComponentCatalog Load(bool reload = false)
        {
            lock (_syncLock)
            {
                if (reload != false || _catalog == null)
                    _catalog = new ComponentCatalog(_loaders.SelectMany(l => l.LoadComponents()).OrderBy(c=>c.Name).ToList());
                
                return _catalog;
            }
        }
    }
}
