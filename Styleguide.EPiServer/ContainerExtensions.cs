using System;
using System.Collections.Generic;
using System.IO;
using EPiServer;
using EPiServer.Construction;
using EPiServer.DataAbstraction;
using Forte.Styleguide.EPiServer.ContentProvider;
using Forte.Styleguide.EPiServer.JsonConverters;
using Newtonsoft.Json;
using StructureMap;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Forte.Styleguide.EPiServer
{

    public static class MyServer
    {
        public static string MapPath(string path)
        {
            var str = Path.Combine((string)AppDomain.CurrentDomain.GetData("ContentRootPath"),
                path);
            return path;
        }
    }

}
