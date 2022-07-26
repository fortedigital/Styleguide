using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json.Linq;
using Styleguide.JsonGenerator.Extensions;


namespace Styleguide.JsonGenerator
{
    [Generator]
    public class StyleguideJsonGenerator : ISourceGenerator
    {
        static StyleguideJsonGenerator()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
            {
                AssemblyName name = new AssemblyName(args.Name);
                Assembly loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().FullName == name.FullName);
                if (loadedAssembly != null)
                {
                    return loadedAssembly;
                }

                string resourceName = $"Styleguide.JsonGenerator.{name.Name}.dll";

                using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (resourceStream == null)
                    {
                        return null;
                    }

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        resourceStream.CopyTo(memoryStream);
                        return Assembly.Load(memoryStream.ToArray());
                    }
                }
            };
        }
        
        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var compilation = (CSharpCompilation)context.Compilation;

            var CourseListProps =
                compilation.GetTypeByMetadataNameOrThrow(
                    "KongsbergMaritime.Web.Features.Pages.CoursePortalPage.CourseListProps");

            var properties =
                CourseListProps.GetAllMembers().OfType<IPropertySymbol>();

            var visitor = new TypeMembersVisitor(compilation);
            var test = properties.Select(prop => prop.Accept(visitor)).ToList();

            // var generator = new JsonGenerator(compilation);
            // generator.Run();
        }
    }
    
    public class TypeMembersVisitor : SymbolVisitor<JProperty>
    {
        private readonly Lazy<INamedTypeSymbol> iEnumerableType;

        #region ValueType types

        private readonly INamedTypeSymbol doubleType;
        private readonly INamedTypeSymbol singleType;
        private readonly INamedTypeSymbol intType;
        private readonly INamedTypeSymbol guidType;
        private readonly INamedTypeSymbol dateTimeType;

        private readonly Dictionary<INamedTypeSymbol, JValue> valueTypesLookup;

        #endregion
        
        public TypeMembersVisitor(CSharpCompilation compilation)
        {
            iEnumerableType = new Lazy<INamedTypeSymbol>(() =>
                compilation.GetTypeByMetadataNameOrThrow("System.Collections.IEnumerable"));

            #region Initialize ValueType types

            doubleType = compilation.GetTypeByMetadataNameOrThrow("System.Double");
            singleType = compilation.GetTypeByMetadataNameOrThrow("System.Single");
            intType = compilation.GetTypeByMetadataNameOrThrow("System.Int32");
            guidType = compilation.GetTypeByMetadataNameOrThrow("System.Guid");
            dateTimeType = compilation.GetTypeByMetadataNameOrThrow("System.DateTime");

#pragma warning disable RS1024
            valueTypesLookup = new Dictionary<INamedTypeSymbol, JValue>
            {
                { doubleType, new JValue(default(double)) },
                { singleType, new JValue(default(float)) },
                { intType, new JValue(default(int)) },
                { guidType, new JValue(Guid.NewGuid()) },
                { dateTimeType, new JValue(DateTime.Now) },
            };
#pragma warning restore RS1024

            #endregion

        }
        
        public override JProperty VisitProperty(IPropertySymbol symbol)
        {
            if (symbol.Type is IArrayTypeSymbol arrayTypeSymbol)
            {
                var arrayElement = arrayTypeSymbol.ElementType.IsFromCodeBase()
                    ? new JObject(
                        new JProperty("ContentType", symbol.Type.Name),
                        arrayTypeSymbol.ElementType.GetAllMembers().OfType<IPropertySymbol>().Select(VisitProperty))
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
                            new JProperty("ContentType", symbol.Type.Name),
                            elementType.GetAllMembers().OfType<IPropertySymbol>().Select(VisitProperty)
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
                    return new JProperty(namedTypeSymbol.Name,
                        valueTypesLookup.TryGetValue(namedTypeSymbol, out var defaultValue)
                            ? defaultValue
                            : new JValue(0));
                }
            }
            
            return new JProperty(
                symbol.Name, symbol.Type.IsFromCodeBase() 
                ? new JObject(
                    new JProperty("ContentType", symbol.Type.Name),
                    symbol.Type.GetAllMembers().OfType<IPropertySymbol>().Select(VisitProperty)
                    ) 
                : new JObject());
            
        }
    }
}
