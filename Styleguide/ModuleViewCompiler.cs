using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Forte.Styleguide
{
    public class ModuleViewCompiler : IViewCompiler
    {
        public static ModuleViewCompiler Current;
        protected ApplicationPartManager ApplicationPartManager { get; }
        protected ILogger Logger { get; }
        protected Dictionary<string, CancellationTokenSource> CancellationTokenSources { get; }
        protected ConcurrentDictionary<string, string> NormalizedPathCache { get; }
        protected Dictionary<string, CompiledViewDescriptor> CompiledViews { get; private set; }

        public ModuleViewCompiler(ApplicationPartManager applicationPartManager, ILoggerFactory loggerFactory)
        {
            ApplicationPartManager = applicationPartManager;
            Logger = loggerFactory.CreateLogger<ModuleViewCompiler>();
            CancellationTokenSources = new Dictionary<string, CancellationTokenSource>();
            NormalizedPathCache = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);
            PopulateCompiledViews();
            Current = this;
        }


        public void LoadModuleCompiledViews(Assembly moduleAssembly)
        {
            if (moduleAssembly == null)
                throw new ArgumentNullException(nameof(moduleAssembly));
            var cancellationTokenSource = new CancellationTokenSource();
            CancellationTokenSources.Add(moduleAssembly.FullName, cancellationTokenSource);
            var feature = new ViewsFeature();
            ApplicationPartManager.PopulateFeature(feature);
            foreach (var compiledView in feature.ViewDescriptors
                .Where(v => v.Type.Assembly == moduleAssembly))
            {
                if (!CompiledViews.ContainsKey(compiledView.RelativePath))
                {
                    compiledView.ExpirationTokens = new List<IChangeToken>() { new CancellationChangeToken(cancellationTokenSource.Token) };
                    CompiledViews.Add(compiledView.RelativePath, compiledView);
                }
            }
        }

        public void UnloadModuleCompiledViews(Assembly moduleAssembly)
        {
            if (moduleAssembly == null)
                throw new ArgumentNullException(nameof(moduleAssembly));
            foreach (KeyValuePair<string, CompiledViewDescriptor> entry in CompiledViews
                .Where(kvp => kvp.Value.Type.Assembly == moduleAssembly))
            {
                CompiledViews.Remove(entry.Key);
            }
            if (CancellationTokenSources.TryGetValue(moduleAssembly.FullName, out CancellationTokenSource cancellationTokenSource))
            {
                cancellationTokenSource.Cancel();
                CancellationTokenSources.Remove(moduleAssembly.FullName);
            }
        }

        private void PopulateCompiledViews()
        {
            var feature = new ViewsFeature();
            ApplicationPartManager.PopulateFeature(feature);
            CompiledViews = new Dictionary<string, CompiledViewDescriptor>(feature.ViewDescriptors.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var compiledView in feature.ViewDescriptors)
            {
                if (CompiledViews.ContainsKey(compiledView.RelativePath))
                    continue;
                CompiledViews.Add(compiledView.RelativePath, compiledView);
            };
        }

        public async Task<CompiledViewDescriptor> CompileAsync(string relativePath)
        {
            if (relativePath == null)
                throw new ArgumentNullException(nameof(relativePath));
            if (CompiledViews.TryGetValue(relativePath, out CompiledViewDescriptor cachedResult))
                return cachedResult;
            var normalizedPath = GetNormalizedPath(relativePath);
            if (CompiledViews.TryGetValue(normalizedPath, out cachedResult))
                return cachedResult;
            return await Task.FromResult(new CompiledViewDescriptor()
            {
                RelativePath = normalizedPath,
                ExpirationTokens = Array.Empty<IChangeToken>(),
            });
        }

        protected string GetNormalizedPath(string relativePath)
        {
            if (relativePath.Length == 0)
                return relativePath;
            if (!NormalizedPathCache.TryGetValue(relativePath, out var normalizedPath))
            {
                normalizedPath = NormalizePath(relativePath);
                NormalizedPathCache[relativePath] = normalizedPath;
            }
            return normalizedPath;
        }

        protected string NormalizePath(string path)
        {
            var addLeadingSlash = path[0] != '\\' && path[0] != '/';
            var transformSlashes = path.IndexOf('\\') != -1;
            if (!addLeadingSlash && !transformSlashes)
                return path;
            var length = path.Length;
            if (addLeadingSlash)
                length++;
            return string.Create(length, (path, addLeadingSlash), (span, tuple) =>
            {
                var (pathValue, addLeadingSlashValue) = tuple;
                int spanIndex = 0;
                if (addLeadingSlashValue)
                    span[spanIndex++] = '/';
                foreach (var ch in pathValue)
                {
                    span[spanIndex++] = ch == '\\' ? '/' : ch;
                }
            });
        }

    }
}
