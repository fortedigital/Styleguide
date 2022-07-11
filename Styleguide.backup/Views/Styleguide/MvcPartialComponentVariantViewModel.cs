using System.Collections.Generic;
using System.Web.Mvc;

namespace Forte.Styleguide
{
    public class MvcPartialComponentVariantViewModel
    {
        public string Name { get; set; }
        public object Model { get; set; }
        public ViewDataDictionary ViewData { get; set; } = new ViewDataDictionary();
    }
}