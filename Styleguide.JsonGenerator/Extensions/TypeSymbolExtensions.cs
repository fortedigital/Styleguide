using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Styleguide.JsonGenerator.Extensions
{
    public static class TypeSymbolExtensions
    {
        public static bool IsDescendantOf(this ITypeSymbol symbol, ITypeSymbol other)
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

        public static bool DoesImplement(this ITypeSymbol symbol, INamedTypeSymbol @interface) =>
            symbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, @interface));

        public static ImmutableArray<ISymbol> GetAllMembers(this ITypeSymbol symbol)
        {
            var retVal = new List<ISymbol>();
            var tmp = symbol.BaseType;

            retVal.AddRange(symbol.GetMembers());
            
            while (tmp != null)
            {
                retVal.AddRange(tmp.GetMembers());
                tmp = tmp.BaseType;
            }

            return retVal.ToImmutableArray();
        }
        
        public static string GetContainingFilePath(this ITypeSymbol symbol) =>
            symbol.Locations.FirstOrDefault()?.GetLineSpan().Path;

        public static bool IsFromCodeBase(this ITypeSymbol symbol) =>
            symbol.Locations.Any(location => location.Kind == LocationKind.SourceFile);
    }
}