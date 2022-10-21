using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly bool UseTags;

        private readonly JsonSerializerSettings serializerSettings;

        public MvcPartialComponentLoader(
            string rootPath, 
            string componentFileNameExtension, 
            string componentMarkdownFileExtension,
            bool useTags,
            JsonSerializerSettings serializerSettings,
            string layoutPath = null)
        {
            this.LayoutPath = layoutPath;
            this.RootPath = rootPath;
            this.ComponentFileNameExtension = componentFileNameExtension;
            this.ComponentMarkdownFileExtension = componentMarkdownFileExtension;
            this.UseTags = useTags;
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
                    LoadTags(initialData, componentCategory),
                    this.LayoutPath,
                    new FileInfo(path),
                    new FileInfo(path.Replace(ComponentFileNameExtension, ComponentMarkdownFileExtension)),
                    this.serializerSettings);
            }
        }

        private IEnumerable<string> LoadTags(InitialData initialData, string componentCategory)
        {
            if (!this.UseTags)
            {
                return new List<string> { componentCategory };
            }
            
            return initialData.Tags ?? new List<string> { CommonTagName };
        }
    }
}
