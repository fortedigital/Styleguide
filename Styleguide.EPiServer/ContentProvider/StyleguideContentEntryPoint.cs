using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace Forte.Styleguide.EPiServer.ContentProvider
{
    [ContentType(GUID = "D7A5AC92-D190-4338-B578-45712540F63B", AvailableInEditMode = false)]
    public class StyleguideContentEntryPoint : PageData
    {
        public static ContentReference Ensure(IContentRepository repository)
        {
            var entryPoint = repository.GetChildren<StyleguideContentEntryPoint>(ContentReference.RootPage).SingleOrDefault(); 
            if (entryPoint == null)
            {
                entryPoint = repository.GetDefault<StyleguideContentEntryPoint>(ContentReference.RootPage);
                entryPoint.Name = "Styleguide Content";
                repository.Save(entryPoint, global::EPiServer.DataAccess.SaveAction.Publish, global::EPiServer.Security.AccessLevel.NoAccess);
            }

            return entryPoint.ContentLink;
        }        
    }
}
