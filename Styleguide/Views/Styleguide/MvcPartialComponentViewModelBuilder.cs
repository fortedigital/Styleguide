using System;
using System.Linq;

namespace Forte.Styleguide
{
    public class MvcPartialComponentViewModelBuilder
    {
        private readonly MvcPartialComponentViewModel viewModel = new MvcPartialComponentViewModel();

        public MvcPartialComponentViewModelBuilder WithName(string name)
        {
            viewModel.Name = name;
            return this;
        }

        public MvcPartialComponentViewModelBuilder WithPartialName(string partialName)
        {
            if (string.IsNullOrEmpty(partialName) == false)
            {
                viewModel.PartialName = partialName;    
            }
            
            return this;
        }

        public MvcPartialComponentViewModelBuilder WithModel(object model)
        {
            viewModel.Model = model;
            return this;
        }

        public MvcPartialComponentViewModelBuilder WithVariant(Action<MvcPartialComponentVariantViewModelBuilder> buildAction)
        {
            var builder = new MvcPartialComponentVariantViewModelBuilder();
            
            buildAction?.Invoke(builder);

            viewModel.Variants = viewModel.Variants.Concat(new[] {builder.Build()});

            return this;
        }

        public MvcPartialComponentViewModelBuilder WithVariants(params MvcPartialComponentVariantViewModel[] variants)
        {
            viewModel.Variants = viewModel.Variants.Concat(variants ?? new MvcPartialComponentVariantViewModel[0]);
            return this;
        }

        public MvcPartialComponentViewModelBuilder WithLayout(string layoutPath)
        {
            viewModel.LayoutPath = layoutPath;

            return this;
        }

        public MvcPartialComponentViewModelBuilder WithCategory(string category)
        {
            viewModel.Category = category;

            return this;
        }

        public MvcPartialComponentViewModel Build()
        {
            return viewModel;
        }
    }
}
