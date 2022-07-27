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
                var targetFileName = source.SourceType.Name.Replace("ViewModel", string.Empty).Replace("Props", String.Empty);
                Debug.Write($"{targetFileName}: ");

                var properties = source.SourceType.GetAllMembers().OfType<IPropertySymbol>()
                    .Where(symbol => symbol.DeclaredAccessibility == Accessibility.Public) // get all public
                    .Where(symbol => !symbol.HasAttribute(_styleguideIgnoreAttributeType.Value)); // that are not marked as StyleguideIgnore

                JObject componentContext;

                var propertyVisitor = new TypeMembersVisitor(Compilation);
                
                try
                {
                    componentContext = 
                        new JObject(
                            new JProperty(
                                "category",
                                source.Category
                            ),
                            new JProperty(
                                "model", 
                                new JObject(
                                    properties.Select(property => property.Accept(propertyVisitor))
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
                }
                catch (Exception e)
                {
                    Exceptions.Add(e);
                    return;
                }
                
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
                if (!sourceProperty.Type.OriginalDefinition.DoesImplement(_systemIEnumerableType.Value))
                {
                    throw new Exception($"[{sourceProperty.ContainingType.Name}] Property {sourceProperty.Name} is not a collection. Use StyleguideContentTypeAttribute instead.");
                }

                var itemsContentTypesAttribute = propertyAttributes.First(attribute =>
                    attribute.AttributeClass?.MetadataName ==
                    _styleguideItemsContentTypesAttributeType.Value.MetadataName);

                var contentTypes = itemsContentTypesAttribute.ConstructorArguments.FirstOrDefault().Values.Select(value => value.Value).Cast<string>();
                
                return new JProperty(sourceProperty.Name, new JArray(contentTypes.Select(CreateJsonObjectForContentType)));
            }

            if (sourceProperty.HasAttribute(_styleguideContentTypeAttributeType.Value))
            {
                if (sourceProperty.Type.OriginalDefinition.DoesImplement(_systemIEnumerableType.Value))
                    throw new Exception($"[{sourceProperty.ContainingType.Name}] Property {sourceProperty.Name} is a collection. Use StyleguideItemsContentTypesAttribute instead.");
                
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
        
        private ITypeSymbol GetModelFromViewModel(ITypeSymbol viewModel) => 
            viewModel.GetAttributes()
                .First(data =>
                    data.AttributeClass?.MetadataName == _styleguideViewModelForAttributeType.Value.MetadataName)
                .ConstructorArguments.First().ConvertToType(Compilation);
        
        private IEnumerable<(ITypeSymbol ViewModel, ITypeSymbol Model)>
            GroupViewModelsWithCorrespondingModels(IEnumerable<ITypeSymbol> viewModels) => viewModels
            .Select(viewModel => (viewModel, GetModelFromViewModel(viewModel)));
        
        private string GetCategory(ITypeSymbol model) => (string) model.GetAttributes()
            .FirstOrDefault(attribute => attribute.AttributeClass?.MetadataName == _epiServerContentTypeAttributeType.Value.MetadataName)?
            .NamedArguments.FirstOrDefault(kvp => kvp.Key == "GroupName").Value.Value ?? StyleguideDefaultCategoryName;
    }
    
    public class TypeMembersVisitor : SymbolVisitor<JProperty>
    {
        private readonly Lazy<INamedTypeSymbol> iEnumerableType;

        private readonly Lazy<INamedTypeSymbol> styleguideIgnoreType;

        #region ValueType types

        private readonly INamedTypeSymbol doubleType;
        private readonly INamedTypeSymbol singleType;
        private readonly INamedTypeSymbol intType;
        private readonly INamedTypeSymbol guidType;
        private readonly INamedTypeSymbol dateTimeType;
        private readonly INamedTypeSymbol booleanType;
        private readonly INamedTypeSymbol stringType;

        private readonly Dictionary<INamedTypeSymbol, JValue> valueTypesLookup;

        #endregion
        
        public TypeMembersVisitor(CSharpCompilation compilation)
        {
            iEnumerableType = new Lazy<INamedTypeSymbol>(() =>
                compilation.GetTypeByMetadataNameOrThrow("System.Collections.IEnumerable"));

            stringType = compilation.GetTypeByMetadataNameOrThrow("System.String");

            #region Initialize Styleguide specific types

            styleguideIgnoreType = new Lazy<INamedTypeSymbol>(() =>
                compilation.GetTypeByMetadataNameOrThrow(
                    "Styleguide.JsonGenerator.Annotations.StyleguideIgnoreAttribute"));

            #endregion

            #region Initialize ValueType types

            doubleType = compilation.GetTypeByMetadataNameOrThrow("System.Double");
            singleType = compilation.GetTypeByMetadataNameOrThrow("System.Single");
            intType = compilation.GetTypeByMetadataNameOrThrow("System.Int32");
            guidType = compilation.GetTypeByMetadataNameOrThrow("System.Guid");
            dateTimeType = compilation.GetTypeByMetadataNameOrThrow("System.DateTime");
            booleanType = compilation.GetTypeByMetadataNameOrThrow("System.Boolean");

#pragma warning disable RS1024
            valueTypesLookup = new Dictionary<INamedTypeSymbol, JValue>
            {
                { doubleType, new JValue(default(double)) },
                { singleType, new JValue(default(float)) },
                { intType, new JValue(default(int)) },
                { guidType, new JValue(Guid.NewGuid()) },
                { dateTimeType, new JValue(DateTime.Now) },
                { booleanType, new JValue(default(bool))}
            };
#pragma warning restore RS1024

            #endregion

        }
        
        public override JProperty VisitProperty(IPropertySymbol symbol)
        {
            if (symbol.Type is IArrayTypeSymbol arrayTypeSymbol)
            {
                var elementType = arrayTypeSymbol.ElementType;
                var arrayElement = elementType.IsFromCodeBase()
                    ? new JObject(
                        GetContentTypeNameProperty(elementType),
                        GetValidMembers(elementType).Select(VisitProperty))
                    : null;

                return new JProperty(symbol.Name, 
                    arrayElement != null 
                    ? new JArray(arrayElement, arrayElement, arrayElement)
                    : new JArray()
                );
            }

            if (symbol.Type is INamedTypeSymbol namedTypeSymbol)
            {
                if (namedTypeSymbol.Arity == 1 && namedTypeSymbol.DoesImplement(iEnumerableType.Value))
                {
                    var elementType = namedTypeSymbol.TypeArguments.First();
                    
                    var arrayElement = elementType.IsFromCodeBase()
                        ? new JObject(
                            GetContentTypeNameProperty(elementType),
                            GetValidMembers(elementType).Select(VisitProperty)
                        )
                        : null;
                    
                    return new JProperty(symbol.Name, 
                        arrayElement != null 
                            ? new JArray(arrayElement, arrayElement, arrayElement)
                            : new JArray()
                    );
                }

                if (namedTypeSymbol.IsValueType)
                {
                    return new JProperty(symbol.Name,
                        valueTypesLookup.TryGetValue(namedTypeSymbol, out var defaultValue)
                            ? defaultValue
                            : new JValue(0));
                }
            }

            return SymbolEqualityComparer.Default.Equals(symbol.Type, stringType)
                ? new JProperty(symbol.Name, "")
                : new JProperty(
                    symbol.Name,
                    new JObject(
                        GetContentTypeNameProperty(symbol.Type),
                        GetValidMembers(symbol.Type).Select(VisitProperty)
                    )
                );
        }
        
        private IEnumerable<IPropertySymbol> GetValidMembers(ITypeSymbol propertyType) => 
            propertyType
                .GetAllMembers()
                .OfType<IPropertySymbol>()
                .Where(type => type.DeclaredAccessibility == Accessibility.Public)
                .Where(symbol => !symbol.HasAttribute(styleguideIgnoreType.Value));

        private JProperty GetContentTypeNameProperty(ITypeSymbol propertyType) =>
            new JProperty("ContentType", propertyType.IsAbstract ? "#PROVIDE_CONCRETE_TYPE#" : propertyType.Name);
        
    }
}