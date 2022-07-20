using System;
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
    }
}