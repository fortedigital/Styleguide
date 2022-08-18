using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using EPiServer.Construction;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAbstraction.RuntimeModel;
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
        private readonly ISharedBlockFactory sharedBlockFactory;
        private readonly IContentFactory contentFactory;

        public StyleguideContentFactory(ContentReference entryPoint, IContentTypeRepository contentTypeRepository, IContentFactory contentFactory, ISharedBlockFactory sharedBlockFactory)
        {
            this.entryPoint = entryPoint;
            this.contentTypeRepository = contentTypeRepository;
            this.contentFactory = contentFactory;
            this.sharedBlockFactory = sharedBlockFactory;
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
            var content = CreateInstance(contentType);
            content.ContentTypeID = contentType.ID;
            content.ParentLink = this.entryPoint;
            content.ContentGuid = Guid.NewGuid();
            content.ContentLink = new ContentReference(id, StyleguideContentProvider.ProviderName);
            content.Name = String.IsNullOrEmpty(name) ? id.ToString() : name;
            
            var securable = content as IContentSecurable;
            securable?.GetContentSecurityDescriptor().AddEntry(new AccessControlEntry(EveryoneRole.RoleName, AccessLevel.Read));

            if (content is ILocalizable localizable)
            {
                localizable.Language = new CultureInfo("no");
                localizable.MasterLanguage = localizable.Language;
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
                    if (properties.TryGetValue("Url", out var url))
                    {
                        var uri = new Uri(url.ToString(), UriKind.RelativeOrAbsolute);
                        
                        media.BinaryData = new WebBlob(uri, uri.ToString());
                        media.Thumbnail = media.BinaryData;
                        if (string.IsNullOrEmpty(name))
                            media.Name = $"{id}_{Path.GetFileName(uri.AbsolutePath)}";
                        else
                            media.Name = name;
                        if (string.IsNullOrEmpty(Path.GetExtension(media.Name)))
                            media.Name = media.Name + ".jpg";
                    }
                    break;
            }

            if (content is IRoutable routable)
            {
                var urlSegmentGenerator = ServiceLocator.Current.GetInstance<IUrlSegmentGenerator>();
                routable.RouteSegment = urlSegmentGenerator.Create(content.Name); // TODO: inject generator
            }

            return content;
        }

        private IContent CreateInstance(ContentType contentType)
        {
            if (!(contentType is BlockType blockType)) return contentFactory.CreateContent(contentType);
            
            var modelType = blockType.ModelType ?? typeof(BlockData);
            return sharedBlockFactory.CreateSharedBlock(modelType);
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