using System;

namespace Styleguide
{
    public class KnockoutComponentViewModel
    {
        public string Name { get; set; }
        public string BootstrapFunctionName => Char.ToLower(this.Name[0]) + this.Name.Substring(1);
    }
}