using System.Collections;
using System.Collections.Generic;

namespace Styleguide
{
    public class ComponentViewModelLoader
    {
        private readonly IComponentJsonDataLoader _dataSourceLoader;
        private readonly IComponentViewModelTypeResolver _viewModelTypeResolver;

        public ComponentViewModelLoader(
            IComponentJsonDataLoader dataSourceLoader, IComponentViewModelTypeResolver viewModelTypeResolver)
        {
            _dataSourceLoader = dataSourceLoader;
            _viewModelTypeResolver = viewModelTypeResolver;
        }

        public IEnumerable LoadManyFromJson(string componentName)
        {
            var componentType = _viewModelTypeResolver.ResolveType(componentName); 
            var collectionType = typeof(List<>).MakeGenericType(componentType);

            var json = _dataSourceLoader.Load(componentName, componentType);
            var collection = (IEnumerable)new ComponentViewModelDeserializer().Deserialize(json, collectionType);

            return collection;
        }
    }
}