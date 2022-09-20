using System.Linq;
using Microsoft.CodeAnalysis;

namespace Styleguide.JsonGenerator.Extensions
{
    public static class SymbolExtensions
    {
        public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attributeSymbol) =>
            symbol.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attributeSymbol, attribute.AttributeClass));
    }
}