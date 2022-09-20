using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;

namespace Forte.Styleguide.EPiServer.ContentProvider
{
    public interface IStyleguideContentRepository
    {
        void AddOrReplace(IContent conent);
    }

    public class StyleguideContentRepository : IStyleguideContentRepository
    {
        private static readonly List<IContent> Items = new List<IContent>();

        public void AddOrReplace(IContent content)
        {
            lock (Items)
            {
                var toBeReplaced = this.Get(content.ContentLink.ID);
                if (toBeReplaced != null)
                {
                    Items.Remove(toBeReplaced);
                }

                Items.Add(content);
            }
        }

        public IContent Get(int id)
        {
            lock (Items)
            {
                return Items.FirstOrDefault(x => x.ContentLink.ID == id);
            }
        }

        public IReadOnlyCollection<IContent> GetAll()
        {
            return Items;
        }
    }
}