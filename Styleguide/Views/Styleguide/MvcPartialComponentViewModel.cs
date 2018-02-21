using System.Collections.Generic;

namespace Forte.Styleguide
{
    public class MvcPartialComponentViewModel
    {
        public string ComponentName;
        public string Error;
        public string PartialName;
        public IEnumerable<object> Variants;
    }
}