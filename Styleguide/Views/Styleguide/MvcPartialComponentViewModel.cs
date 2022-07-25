namespace Styleguide.Views.Styleguide
{
    public class MvcPartialComponentViewModel
    {
        public string Name;
        public string Error;
        public string PartialName;
        public object Model { get; set; }
        public string LayoutPath { get; set; } 
        
        public IEnumerable<MvcPartialComponentVariantViewModel> Variants = Enumerable.Empty<MvcPartialComponentVariantViewModel>();
    }
}
