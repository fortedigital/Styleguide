using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace Styleguide.JsonGenerator.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<INamedTypeSymbol> GetAllDescendantsOf(this IEnumerable<INamedTypeSymbol> list,
            INamedTypeSymbol symbol) => list.Where(typeSymbol => typeSymbol.IsDescendantOf(symbol));

        public static IEnumerable<INamedTypeSymbol> GetAllNonAbstract(this IEnumerable<INamedTypeSymbol> list) =>
            list.Where(typeSymbol => !typeSymbol.IsAbstract);

        public static IEnumerable<INamedTypeSymbol> GetAllFromCodeBase(this IEnumerable<INamedTypeSymbol> list) =>
            list.Where(typeSymbol => typeSymbol.Locations.Any(location => location.Kind == LocationKind.SourceFile));

        public static IEnumerable<INamedTypeSymbol> GetAllWithAttribute(this IEnumerable<INamedTypeSymbol> list,
            INamedTypeSymbol attribute) =>
            list.Where(typeSymbol => typeSymbol.HasAttribute(attribute));
    }
}