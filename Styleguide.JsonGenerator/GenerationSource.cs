using Microsoft.CodeAnalysis;

namespace Styleguide.JsonGenerator
{
    public class GenerationSource
    {
        public GenerationSource(INamedTypeSymbol sourceType, string category)
        {
            SourceType = sourceType;
            Category = category;
        }

        public INamedTypeSymbol SourceType { get; }
        public string Category { get; }
    }
}