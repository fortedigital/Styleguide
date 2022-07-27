namespace Styleguide.Views.Styleguide
{
    public class MvcPartialComponentVariantViewModelBuilder
    {
        private readonly MvcPartialComponentVariantViewModel viewModel = new();

        public MvcPartialComponentVariantViewModelBuilder WithModel(object model)
        {
            viewModel.Model = model;
            return this;
        }

        public MvcPartialComponentVariantViewModel Build()
        {
            return viewModel;
        }
    }
}