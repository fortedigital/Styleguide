using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using EPiServer;
using EPiServer.Configuration;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace Forte.Styleguide.EPiServer.ContentProvider
{
    public class StyleguideContentProvider : global::EPiServer.Core.ContentProvider
    {
        public static string ProviderName => "Mock";

        public static StyleguideContentProvider EnsureInstalled()
        {
            return instance.Value;
        }

        private static readonly Lazy<StyleguideContentProvider> instance = new Lazy<StyleguideContentProvider>(Install, LazyThreadSafetyMode.ExecutionAndPublication);

        private static StyleguideContentProvider Install()
        {
            var provider = new StyleguideContentProvider(new StyleguideContentRepository());

            var entryPoint = StyleguideContentEntryPoint.Ensure(ServiceLocator.Current.GetInstance<IContentRepository>());

            provider.Initialize(ProviderName, new NameValueCollection
            {
                { "entryPoint", entryPoint.ToString() },
                {"capabilities", ContentProviderCapabilities.None.ToString() }
            });

            var providerManager = ServiceLocator.Current.GetInstance<IContentProviderManager>();
            providerManager.ProviderMap.AddProvider(provider);

            return provider;
        }

        private readonly StyleguideContentRepository contentRepository;

        private StyleguideContentProvider(StyleguideContentRepository contentRepository)
        {
            this.contentRepository = contentRepository;
        }

        protected override IContent LoadContent(ContentReference contentLink, ILanguageSelector languageSelector)
        {
            return this.GetContent(contentLink);
        }

        protected override IList<GetChildrenReferenceResult> LoadChildrenReferencesAndTypes(ContentReference contentLink, string languageId, out bool languageSpecific)
        {
            languageSpecific = true;

            var result = this.contentRepository.GetAll()
                .Select(x => new GetChildrenReferenceResult
                {
                    ContentLink = x.ContentLink,
                    ModelType = x.GetType(),
                    IsLeafNode = false
                }).ToList();

            return result;
        }

        protected override ContentResolveResult ResolveContent(ContentReference contentLink)
        {
            var content = this.GetContent(contentLink);

            return new ContentResolveResult
            {
                ContentLink = contentLink,
                UniqueID = Guid.NewGuid(),
                ContentUri = this.ConstructContentUri(content.ContentTypeID, content.ContentLink, content.ContentGuid)
            };
        }

        private IContent GetContent(ContentReference reference)
        {
            var content = this.contentRepository.Get(reference.ID);
            if (content == null)
            {
                throw new InvalidOperationException(
                    $"Unable to create content with id = {reference.ID}.");
            }

            return content;
        }

        protected override void SetCacheSettings(ContentReference contentReference, IEnumerable<GetChildrenReferenceResult> children, CacheSettings cacheSettings)
        {
            cacheSettings.CancelCaching = true;
        }

        protected override void SetCacheSettings(ContentReference parentLink, string urlSegment, IEnumerable<MatchingSegmentResult> childrenMatches, CacheSettings cacheSettings)
        {
            cacheSettings.CancelCaching = true;
        }

        protected override void SetCacheSettings(IContent content, CacheSettings cacheSettings)
        {
            cacheSettings.CancelCaching = true;
        }
    }
}