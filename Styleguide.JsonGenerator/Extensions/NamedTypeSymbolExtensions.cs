using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Styleguide.JsonGenerator.Extensions
{
    public static class NamedTypeSymbolExtensions
    {
        public static string GetContainingFilePath(this INamedTypeSymbol symbol) =>
            symbol.Locations.FirstOrDefault()?.GetLineSpan().Path;
    }
}