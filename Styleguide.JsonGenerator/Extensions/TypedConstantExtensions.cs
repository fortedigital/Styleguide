﻿using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Styleguide.JsonGenerator.Extensions
{
    public static class TypedConstantExtensions
    {
        private const string TypeSymbolNameFromCSharpStringPattern = @"^{typeof\((?<typeSymbolMetadataName>.+)\)}$";
        
        public static INamedTypeSymbol ConvertToType(this TypedConstant typedConstant, CSharpCompilation compilation)
        {
            var regex = new Regex(TypeSymbolNameFromCSharpStringPattern);
            var modelMetadataName = regex.Match(typedConstant.ToCSharpString()).Groups["typeSymbolMetadataName"].Value;

            return compilation.GetTypeByMetadataNameOrThrow(modelMetadataName);
        }
    }
}