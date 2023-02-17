using Microsoft.AspNetCore.Mvc.ViewEngines;
using Newtonsoft.Json;

namespace Forte.Styleguide
{
    public class MvcPartialComponentLoader : IStyleguideComponentLoader
    {
        public string LayoutPath { get; }
        public readonly string RootPath;
        public readonly string ComponentFileNameExtension;
        
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly IViewEngine _engine;

        public MvcPartialComponentLoader(string rootPath, 
            string componentFileNameExtension, 
            JsonSerializerSettings serializerSettings, 
            IViewEngine engine, 
            string layoutPath = null)
        {
            LayoutPath = layoutPath;
            RootPath = rootPath;
            ComponentFileNameExtension = componentFileNameExtension;
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
                componentCategory,
                LayoutPath,
                new FileInfo(path),
                _serializerSettings,
                _engine);
        }
    }
}
