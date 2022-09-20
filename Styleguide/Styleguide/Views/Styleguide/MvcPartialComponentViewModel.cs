using System.Collections.Generic;
using System.Linq;

namespace Forte.Styleguide
{
    public class MvcPartialComponentViewModel
    {
        public string Name;
        public string Error;
        public string PartialName;
        public string Category { get; set; }
        public object Model { get; set; }
        public string LayoutPath { get; set; } 
        
        public IEnumerable<MvcPartialComponentVariantViewModel> Variants = Enumerable.Empty<MvcPartialComponentVariantViewModel>();
    }
}
