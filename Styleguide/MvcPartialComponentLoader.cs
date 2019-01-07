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
        
        private readonly JsonSerializerSettings serializerSettings;

        public MvcPartialComponentLoader(string rootPath, string componentFileNameExtension, JsonSerializerSettings serializerSettings, string layoutPath = null)
        {
            this.LayoutPath = layoutPath;
            this.RootPath = rootPath;
            this.ComponentFileNameExtension = componentFileNameExtension;
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
            var componentName = Path.GetFileName(path).RemoveSuffix(this.ComponentFileNameExtension);
            var componentCategory = Path.GetDirectoryName(path)
                .Split(Path.DirectorySeparatorChar)
                .Reverse()
                .SkipWhile(d => d.Equals(componentName, StringComparison.OrdinalIgnoreCase))
                .First();
            
            return new MvcPartialComponentDescriptor(
                componentName,
                componentCategory,
                this.LayoutPath,
                new FileInfo(path),
                this.serializerSettings);
        }
    }
}
