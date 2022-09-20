using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Styleguide.JsonGenerator.Exceptions;

namespace Styleguide.JsonGenerator.Extensions
{
    public static class CSharpCompilationExtensions
    {
        public static IReadOnlyList<INamedTypeSymbol> GetAllTypes(this CSharpCompilation compilation)
        {
            var result = new List<INamedTypeSymbol>();
            GetAllTypes(result, compilation.GlobalNamespace);
            return result;
        }

        public static INamedTypeSymbol GetTypeByMetadataNameOrThrow(this CSharpCompilation compilation, string metadataName) =>
            compilation.GetTypeByMetadataName(metadataName)
            ?? throw new TypeNotFoundException($"Unable to find type with metadata name: {metadataName} within the assembly");
        
        private static void GetAllTypes(List<INamedTypeSymbol> result, INamespaceOrTypeSymbol symbol)
        {
            if(symbol is INamedTypeSymbol typeSymbol)
                result.Add(typeSymbol);

            foreach (var member in symbol.GetMembers())
            {
                if (member is INamespaceOrTypeSymbol ntSymbol)
                    GetAllTypes(result, ntSymbol);
            }
        }
    }
}

