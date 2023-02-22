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
        public readonly string ComponentMarkdownFileExtension;

        private readonly bool _useTags;
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly IViewEngine _engine;
        private readonly InitialDataDeserializer _initialDataDeserializer;

        public MvcPartialComponentLoader(string rootPath, 
            string componentFileNameExtension, 
            string componentMarkdownFileExtension,
            bool useTags,
            JsonSerializerSettings serializerSettings, 
            IViewEngine engine, 
            string layoutPath = null)
        {
            LayoutPath = layoutPath;
            RootPath = rootPath;
            ComponentFileNameExtension = componentFileNameExtension;
            ComponentMarkdownFileExtension = componentMarkdownFileExtension;
            _useTags = useTags;
            _serializerSettings = serializerSettings;
            _engine = engine;
            _initialDataDeserializer = new InitialDataDeserializer(_serializerSettings);
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
            var initialData = _initialDataDeserializer.Deserialize(jsonContent);

            return new MvcPartialComponentDescriptor(
                componentName,
                string.IsNullOrWhiteSpace(initialData.DisplayName) ? componentName : initialData.DisplayName,
                LoadTags(initialData, componentCategory),
                LayoutPath,
                new FileInfo(path),
                new FileInfo(path.Replace(ComponentFileNameExtension, ComponentMarkdownFileExtension)),
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
