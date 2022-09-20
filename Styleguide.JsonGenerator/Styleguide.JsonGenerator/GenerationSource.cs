using Microsoft.CodeAnalysis;

namespace Styleguide.JsonGenerator
{
    public class GenerationSource
    {
        public GenerationSource(ITypeSymbol sourceType, string category)
        {
            SourceType = sourceType;
            Category = category;
        }

        public ITypeSymbol SourceType { get; }
        public string Category { get; }
    }
}