using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Forte.Styleguide
{
    public class MvcPartialComponentLoader : IStyleguideComponentLoader
    {
        public string LayoutPath { get; }
        public readonly string RootPath;
        public readonly string ComponentFileNameExtension;
        public readonly string ComponentMarkdownFileExtension;
        
        private readonly JsonSerializerSettings serializerSettings;

        public MvcPartialComponentLoader(
            string rootPath, 
            string componentFileNameExtension, 
            string componentMarkdownFileExtension,
            JsonSerializerSettings serializerSettings,
            string layoutPath = null)
        {
            this.LayoutPath = layoutPath;
            this.RootPath = rootPath;
            this.ComponentFileNameExtension = componentFileNameExtension;
            this.ComponentMarkdownFileExtension = componentMarkdownFileExtension;
            this.serializerSettings = serializerSettings;
        }

        public IEnumerable<IStyleguideComponentDescriptor> LoadComponents()
        {
            return Directory
                .GetFiles(this.RootPath, $"*{this.ComponentFileNameExtension}", SearchOption.AllDirectories)
                .Select(this.CreateComponentDescriptorFromFullPath);
        }

        private MvcPartialComponentDescriptor CreateComponentDescriptorFromFullPath(string path)
        {
            var componentName = Path.GetFileName(path).RemoveSuffix(ComponentFileNameExtension);
            var componentCategory = Path.GetDirectoryName(path)
                .Split(Path.DirectorySeparatorChar)
                .Reverse()
                .SkipWhile(d => d.Equals(componentName, StringComparison.OrdinalIgnoreCase))
                .First();

            using (var reader = new FileInfo(path).OpenText())
            {
                var jsonContent = reader.ReadToEnd();
                var initialData = InitialDataDeserializer.Deserialize(jsonContent, this.serializerSettings);
                
                return new MvcPartialComponentDescriptor(
                    componentName,
                    initialData.DisplayName ?? componentName,
                    GetTags(initialData, componentCategory),
                    this.LayoutPath,
                    new FileInfo(path),
                    new FileInfo(path.Replace(ComponentFileNameExtension, ComponentMarkdownFileExtension)),
                    this.serializerSettings);
            }
        }

        private IEnumerable<string> GetTags(InitialData initialData, string componentCategory)
        {
            var tags = new List<string> { componentCategory };
            if (initialData.Tags != null)
            {
                tags.AddRange(initialData.Tags);
            }

            return tags;
        }
    }
}
