using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Forte.Styleguide
{
    public class MvcPartialComponentLoader : IStyleguideComponentLoader
    {
        public readonly string RootPath;
        public readonly string ComponentFileNameExtension;
        
        private readonly JsonSerializerSettings serializerSettings;

        public MvcPartialComponentLoader(string rootPath, string componentFileNameExtension, JsonSerializerSettings serializerSettings)
        {
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
            return new MvcPartialComponentDescriptor(
                Path.GetFileName(path).RemoveSuffix(this.ComponentFileNameExtension),
                new FileInfo(path),
                this.serializerSettings);
        }
    }
}