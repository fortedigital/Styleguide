using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using EPiServer.Construction;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Framework.Blobs;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;

namespace Forte.Styleguide.EPiServer.ContentProvider
{
    public interface IStyleguideContentFactory
    {
        IContent CreateContent(int id, string name, string contentTypeName, IDictionary<string, object> properties);
        IContent CreateContent(int id, string name, Type modelType, IDictionary<string, object> properties);
    }
    
    public class StyleguideContentFactory : IStyleguideContentFactory
    {
        private readonly ContentReference entryPoint;

        private readonly IContentTypeRepository contentTypeRepository;
        private readonly IContentFactory contentFactory;

        public StyleguideContentFactory(ContentReference entryPoint, IContentTypeRepository contentTypeRepository, IContentFactory contentFactory)
        {
            this.entryPoint = entryPoint;
            this.contentTypeRepository = contentTypeRepository;
            this.contentFactory = contentFactory;
        }

        public IContent CreateContent(int id, string name, string contentTypeName, IDictionary<string, object> properties)
        {
            var contentType = this.contentTypeRepository.Load(contentTypeName);
            if (contentType == null)
            {
                throw new InvalidOperationException($"Unable to load content type for type {contentTypeName}");
            }

            return CreateContent(id, name, contentType, properties);
        }

        public IContent CreateContent(int id, string name, Type modelType, IDictionary<string, object> properties)
        {
            var contentType = this.contentTypeRepository.Load(modelType);
            if (contentType == null)
            {
                throw new InvalidOperationException($"Unable to load content type for type {modelType}");
            }

            return CreateContent(id, name, contentType, properties);
        }
        
        private IContent CreateContent(int id, string name, ContentType contentType, IDictionary<string, object> properties)
        {
            var content = this.contentFactory.CreateContent(contentType);
            content.ContentTypeID = contentType.ID;
            content.ParentLink = this.entryPoint;
            content.ContentGuid = Guid.NewGuid();
            content.ContentLink = new ContentReference(id, StyleguideContentProvider.ProviderName);
            content.Name = name;
            if (content is IRoutable)
                ((IRoutable) content).RouteSegment =
                    ServiceLocator.Current.GetInstance<IUrlSegmentGenerator>().Create(content.Name); // TODO: inject generator

            var securable = content as IContentSecurable;
            securable?.GetContentSecurityDescriptor().AddEntry(new AccessControlEntry(EveryoneRole.RoleName, AccessLevel.Read));

            if (content is ILocalizable localizable)
            {
                localizable.Language = new CultureInfo("no");
            }

            if (content is IVersionable versionable)
            {
                versionable.Status = VersionStatus.Published;
            }

            if (content is IChangeTrackable changeTrackable)
            {
                changeTrackable.Changed = DateTime.Now;
            }

            switch (content)
            {
                case PageData page:
                    page.LinkType = PageShortcutType.Normal;
                    break;
                
                case MediaData media:
                    object url;
                    if (properties.TryGetValue("Url", out url))
                    {
                        media.BinaryData = new WebBlob(new Uri(url.ToString(), UriKind.RelativeOrAbsolute), url.ToString());
                        media.Thumbnail = media.BinaryData;
                    }
                    break;
            }

            return content;
        }

        private class WebBlob : Blob
        {
            public string Url { get; }

            public WebBlob(Uri id, string url) : base(id)
            {
                Url = url;
            }

            public override Stream OpenRead()
            {
                return new MemoryStream(new WebClient().DownloadData(this.Url));
            }
        }
    }
}