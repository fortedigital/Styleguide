using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Styleguide.JsonGenerator.Extensions
{
    public static class NamedTypeSymbolExtensions
    {
        public static bool IsDescendantOf(this INamedTypeSymbol symbol, INamedTypeSymbol other)
        {
            if (other is null) throw new ArgumentNullException(nameof(other), "Argument cannot be null");
            if (other.MetadataName == symbol.MetadataName) return false;

            var tmp = symbol;
            while (tmp != null)
            {
                if (tmp.MetadataName == other.MetadataName)
                    return true;

                tmp = tmp.BaseType;
            }

            return false;
        }

        public static bool HasAttribute(this INamedTypeSymbol symbol, INamedTypeSymbol attributeSymbol) =>
            symbol.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attributeSymbol, attribute.AttributeClass));
        
        
        public static string GetType(this INamedTypeSymbol symbol, INamedTypeSymbol attributeSymbol)
        {
            return symbol.GetAttributes().First().ConstructorArguments.FirstOrDefault().Type.MetadataName;
        }

        public static string GetContainingFilePath(this INamedTypeSymbol symbol) =>
            symbol.Locations.FirstOrDefault()?.GetLineSpan().Path;
    }
}