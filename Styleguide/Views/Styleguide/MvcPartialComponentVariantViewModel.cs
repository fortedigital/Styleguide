using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Forte.Styleguide.Views.Styleguide
{
    public class MvcPartialComponentVariantViewModel
    {
        public string Name { get; set; }
        public object Model { get; set; }
        public ViewDataDictionary ViewData { get; set; } = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
    }
}
