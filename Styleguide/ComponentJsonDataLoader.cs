using System;
using System.IO;
using System.Web.Mvc;

namespace Styleguide
{
    public class ComponentJsonDataLoader : IComponentJsonDataLoader
    {
        private readonly ControllerContext context;
        public ComponentJsonDataLoader(ControllerContext context) {
            this.context = context;
        }
        private string ResolveCongigFilePath(string componentName) {
            var engineResult = ViewEngines.Engines.FindPartialView(this.context, componentName);

            if (engineResult.View == null || engineResult.View is RazorView == false)
            {
                throw new InvalidOperationException($"Unable to find razor view for component {componentName}");
            }

            var dirPath = Path.GetDirectoryName(((RazorView)engineResult.View).ViewPath);

            var physicalPath = System.Web.Hosting.HostingEnvironment.MapPath(dirPath+"/"+componentName+".styleguide.json");

            if (string.IsNullOrEmpty(physicalPath))
            {
                throw new InvalidOperationException($"Unable to map virtual path {physicalPath}");
            }

            return physicalPath;
        }
        
        public string Load(string componentName, Type componentType)
        {
            var configFilePath = "";

            try {
                configFilePath = this.ResolveCongigFilePath(componentName);
            } catch (InvalidOperationException e) {
                throw e;
            }

            if (File.Exists(configFilePath) == false )
            {
                throw new InvalidOperationException($"Unable to load component config file {configFilePath}");
            }

            return File.ReadAllText(configFilePath);
        }
    }

    public interface IComponentJsonDataLoader
    {
        string Load(string componentName, Type componentType);
    }
}