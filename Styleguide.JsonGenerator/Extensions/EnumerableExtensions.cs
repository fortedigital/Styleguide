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
        public static IEnumerable<ITypeSymbol> GetAllDescendantsOf(this IEnumerable<ITypeSymbol> list,
            ITypeSymbol symbol) => list.Where(typeSymbol => typeSymbol.IsDescendantOf(symbol));

        public static IEnumerable<ITypeSymbol> GetAllNonAbstract(this IEnumerable<ITypeSymbol> list) =>
            list.Where(typeSymbol => !typeSymbol.IsAbstract);

        public static IEnumerable<ITypeSymbol> GetAllFromCodeBase(this IEnumerable<ITypeSymbol> list) =>
            list.Where(type => type.IsFromCodeBase());

        public static IEnumerable<ITypeSymbol> GetAllWithAttribute(this IEnumerable<ITypeSymbol> list,
            INamedTypeSymbol attribute) =>
            list.Where(typeSymbol => typeSymbol.HasAttribute(attribute));
    }
}