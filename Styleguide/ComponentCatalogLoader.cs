using System.Collections.Generic;
using System.Linq;

namespace Forte.Styleguide
{
    public class ComponentCatalogLoader
    {
        private ComponentCatalog catalog;

        private readonly object syncLock = new object();        
        private readonly IEnumerable<IStyleguideComponentLoader> loaders;

        public ComponentCatalogLoader(IEnumerable<IStyleguideComponentLoader> loaders)
        {
            this.loaders = loaders;
        }

        public ComponentCatalog Load(bool reload = false)
        {
            lock (syncLock)
            {
                if (reload || catalog == null)
                    catalog = new ComponentCatalog(this.loaders.SelectMany(l => l.LoadComponents()).OrderBy(c=>c.DisplayName).ToList());
                
                return catalog;
            }
        }
    }
}