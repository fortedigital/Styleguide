using Microsoft.AspNetCore.Mvc.ViewEngines;
using Newtonsoft.Json;

namespace Forte.Styleguide
{
    public class MvcPartialComponentLoader : IStyleguideComponentLoader
    {
        private const string CommonTagName = "Common";
        
        public string LayoutPath { get; }
        public readonly string RootPath;
        public readonly string ComponentFileNameExtension;
        
        private readonly bool _useTags;
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly IViewEngine _engine;

        public MvcPartialComponentLoader(string rootPath, 
            string componentFileNameExtension, 
            bool useTags,
            JsonSerializerSettings serializerSettings, 
            IViewEngine engine, 
            string layoutPath = null)
        {
            LayoutPath = layoutPath;
            RootPath = rootPath;
            ComponentFileNameExtension = componentFileNameExtension;
            _useTags = useTags;
            _serializerSettings = serializerSettings;
            _engine = engine;
        }

        public IEnumerable<IStyleguideComponentDescriptor> LoadComponents()
        {
            return Directory
                .GetFiles(RootPath, $"*{ComponentFileNameExtension}", SearchOption.AllDirectories)
                .Select(CreateComponentDescriptorFromFullPath);
        }

        private MvcPartialComponentDescriptor CreateComponentDescriptorFromFullPath(string path)
        {
            var componentName = Path.GetFileName(path).RemoveSuffix(ComponentFileNameExtension);
            var componentCategory = Path.GetDirectoryName(path)
                .Split(Path.DirectorySeparatorChar)
                .Reverse()
                .SkipWhile(d => d.Equals(componentName, StringComparison.OrdinalIgnoreCase))
                .First();

            using var reader = new FileInfo(path).OpenText();
            var jsonContent = reader.ReadToEnd();
            var initialData = InitialDataDeserializer.Deserialize(jsonContent, _serializerSettings);

            return new MvcPartialComponentDescriptor(
                componentName,
                string.IsNullOrWhiteSpace(initialData.DisplayName) ? componentName : initialData.DisplayName,
                LoadTags(initialData, componentCategory),
                LayoutPath,
                new FileInfo(path),
                _serializerSettings,
                _engine);
        }
        
        private IEnumerable<string> LoadTags(InitialData initialData, string componentCategory)
        {
            if (!_useTags)
                return new List<string> { componentCategory };

            if (initialData.Tags.Any())
                return initialData.Tags;

            return new [] { CommonTagName };
        }
    }
}
