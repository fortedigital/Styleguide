using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json.Linq;
using Styleguide.JsonGenerator.Extensions;

namespace Styleguide.JsonGenerator
{
    public class JsonGenerator : JsonGeneratorBase
    {
        #region Constants

        private const string SystemIEnumerableTypeMetadataName = "System.Collections.IEnumerable";
        
        private const string EPiServerContentTypeAttributeTypeMetadataName = "EPiServer.DataAnnotations.ContentTypeAttribute";
        
        private const string StyleguideViewModelForAttributeTypeMetadataName = "Styleguide.JsonGenerator.Annotations.StyleguideViewModelForAttribute";
        private const string StyleguideContentTypeAttributeTypeMetadataName = "Styleguide.JsonGenerator.Annotations.StyleguideContentTypeAttribute";
        private const string StyleguideIgnoreAttributeTypeMetadataName = "Styleguide.JsonGenerator.Annotations.StyleguideIgnoreAttribute";
        private const string StyleguideItemsContentTypesAttributeTypeMetadataName = "Styleguide.JsonGenerator.Annotations.StyleguideItemsContentTypesAttribute";
        
        private const string StyleguideJsonFileExtensions = ".styleguide.json";

        private const string StyleguideDefaultCategoryName = "Default";
        #endregion
        
        private readonly Lazy<INamedTypeSymbol> _systemIEnumerableType;
        private readonly Lazy<INamedTypeSymbol> _epiServerContentTypeAttributeType;
        private readonly Lazy<INamedTypeSymbol> _styleguideViewModelForAttributeType;
        private readonly Lazy<INamedTypeSymbol> _styleguideContentTypeAttributeType;
        private readonly Lazy<INamedTypeSymbol> _styleguideItemsContentTypesAttributeType;
        private readonly Lazy<INamedTypeSymbol> _styleguideIgnoreAttributeType;

        

        public JsonGenerator(CSharpCompilation compilation) : base(compilation)
        {
            _styleguideViewModelForAttributeType = new Lazy<INamedTypeSymbol>(() =>
                compilation.GetTypeByMetadataNameOrThrow(StyleguideViewModelForAttributeTypeMetadataName));
            
            _systemIEnumerableType = new Lazy<INamedTypeSymbol>(() =>
                compilation.GetTypeByMetadataNameOrThrow(SystemIEnumerableTypeMetadataName));
            
            _epiServerContentTypeAttributeType = new Lazy<INamedTypeSymbol>(() =>
                compilation.GetTypeByMetadataNameOrThrow(EPiServerContentTypeAttributeTypeMetadataName));

            _styleguideContentTypeAttributeType = new Lazy<INamedTypeSymbol>(() =>
                compilation.GetTypeByMetadataNameOrThrow(StyleguideContentTypeAttributeTypeMetadataName));

            _styleguideIgnoreAttributeType = new Lazy<INamedTypeSymbol>(() =>
                compilation.GetTypeByMetadataNameOrThrow(StyleguideIgnoreAttributeTypeMetadataName));
            
            _styleguideItemsContentTypesAttributeType = new Lazy<INamedTypeSymbol>(() =>
                compilation.GetTypeByMetadataNameOrThrow(StyleguideItemsContentTypesAttributeTypeMetadataName));
        }

        protected override IEnumerable<GenerationSource> GetSourceFiles()
        {
            var allTypes = Compilation.GetAllTypes();
            var blockViewModels = allTypes.GetAllNonAbstract().GetAllFromCodeBase()
                .GetAllWithAttribute(_styleguideViewModelForAttributeType.Value);

            return GroupViewModelsWithCorrespondingModels(blockViewModels).Select(viewModelModelPair => new GenerationSource(viewModelModelPair.ViewModel, GetCategory(viewModelModelPair.Model)));
        }

        protected override void GenerateJsonFiles(IEnumerable<GenerationSource> sourceFiles) => sourceFiles
            .AsParallel()
            .ForAll(source =>
            {
                var targetDirectory = Directory.GetParent(source.SourceType.GetContainingFilePath()) ?? throw new ArgumentNullException();
                var targetFileName = source.SourceType.Name.Replace("ViewModel", string.Empty).Replace("Block", string.Empty);
                Debug.Write($"{targetFileName}: ");

                var properties = source.SourceType.GetMembers().OfType<IPropertySymbol>()
                    .Where(symbol => symbol.DeclaredAccessibility == Accessibility.Public) // get all public
                    .Where(symbol => !symbol.HasAttribute(_styleguideIgnoreAttributeType.Value)); // that are not marked as StyleguideIgnore
                
                var componentContext = 
                    new JObject(
                        new JProperty(
                            "category",
                            source.Category
                            ),
                        new JProperty(
                            "model", 
                            new JObject(
                                properties.Select(CreateProperty)
                                )
                        ),
                        new JProperty(
                            "variants", 
                            new JArray(
                                    new JObject(
                                        new JProperty("name", "Standard"),
                                        new JProperty("model", new JObject())
                                        )
                                )
                            )
                    );
                
                try
                {
                    using (var fs = new FileStream($"{Path.Combine(targetDirectory.FullName, $"{targetFileName}{StyleguideJsonFileExtensions}")}", FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    {
                        Debug.WriteLine("CREATED");
                        using (var streamWriter = new StreamWriter(fs))
                        {
                            streamWriter.Write(componentContext.ToString());
                        }
                    }
                }
                catch
                {
                    Debug.WriteLine("SKIPPED");
                    // ignored
                }
            });

        private JProperty CreateProperty(IPropertySymbol sourceProperty)
        {
            var propertyAttributes = sourceProperty.GetAttributes();
            
            if (sourceProperty.HasAttribute(_styleguideItemsContentTypesAttributeType.Value))
            {
                if (!sourceProperty.Type.IsDescendantOf(_systemIEnumerableType.Value))
                    Exceptions.Add(new Exception($"Property ${sourceProperty.Name} is not a collection. Use StyleguideContentTypeAttribute instead."));

                var itemsContentTypesAttribute = propertyAttributes.First(attribute =>
                    attribute.AttributeClass?.MetadataName ==
                    _styleguideItemsContentTypesAttributeType.Value.MetadataName);

                var contentTypes = itemsContentTypesAttribute.ConstructorArguments.Select(arg => arg.Value).Cast<string>();
                
                return new JProperty(sourceProperty.Name, new JArray(contentTypes.Select(CreateJsonObjectForContentType)));
            }

            if (sourceProperty.HasAttribute(_styleguideContentTypeAttributeType.Value))
            {
                if (sourceProperty.Type.IsDescendantOf(_systemIEnumerableType.Value))
                    Exceptions.Add(new Exception($"Property ${sourceProperty.Name} is a collection. Use StyleguideItemsContentTypesAttribute instead."));
                
                var contentTypeAttribute = propertyAttributes.First(attribute =>
                    attribute.AttributeClass?.MetadataName ==
                    _styleguideContentTypeAttributeType.Value.MetadataName);

                var contentType = contentTypeAttribute.ConstructorArguments.Take(1).Select(x => x.Value).Cast<string>();
                
                return new JProperty(sourceProperty.Name, contentType.Select(CreateJsonObjectForContentType));
            }

            return new JProperty(sourceProperty.Name, string.Empty);
        }

        private JObject CreateJsonObjectForContentType(string contentType) =>
            new JObject(
                new JProperty("contentType", contentType)
            );
        
        // private JArray CreateJsonArrayForCollection(string[] contentTypes)
        
        private INamedTypeSymbol GetModelFromViewModel(INamedTypeSymbol viewModel) => 
            viewModel.GetAttributes()
                .First(data =>
                    data.AttributeClass?.MetadataName == _styleguideViewModelForAttributeType.Value.MetadataName)
                .ConstructorArguments.First().ConvertToType(Compilation);
        
        private IEnumerable<(INamedTypeSymbol ViewModel, INamedTypeSymbol Model)>
            GroupViewModelsWithCorrespondingModels(IEnumerable<INamedTypeSymbol> viewModels) => viewModels
            .Select(viewModel => (viewModel, GetModelFromViewModel(viewModel)));
        
        private string GetCategory(INamedTypeSymbol model) => (string) model.GetAttributes()
            .FirstOrDefault(attribute => attribute.AttributeClass?.MetadataName == _epiServerContentTypeAttributeType.Value.MetadataName)?
            .NamedArguments.FirstOrDefault(kvp => kvp.Key == "GroupName").Value.Value ?? StyleguideDefaultCategoryName;
    }
}