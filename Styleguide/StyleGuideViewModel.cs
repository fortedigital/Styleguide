using System.Collections.Generic;

namespace Styleguide
{
    public class StyleGuideViewModel
    {
        public IEnumerable<string> PartialComponents { get; set; } = new List<string>();
        public IEnumerable<string> KnockoutComponents { get; set; } = new List<string>();
    }
}