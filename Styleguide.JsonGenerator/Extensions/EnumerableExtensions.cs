using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Styleguide.JsonGenerator.Extensions
{
    public static class EnumerableExtensions
    {
        private const string StyleguideJsonFileExtension = ".styleguide.json";
        
        public static IEnumerable<INamedTypeSymbol> GetAllDescendantsOf(this IEnumerable<INamedTypeSymbol> list,
            INamedTypeSymbol symbol) => list.Where(typeSymbol => typeSymbol.IsDescendantOf(symbol));

        public static IEnumerable<INamedTypeSymbol> GetAllNonAbstract(this IEnumerable<INamedTypeSymbol> list) =>
            list.Where(typeSymbol => !typeSymbol.IsAbstract);

        public static IEnumerable<INamedTypeSymbol> GetAllFromCodeBase(this IEnumerable<INamedTypeSymbol> list) =>
            list.Where(typeSymbol => typeSymbol.Locations.Any(location => location.Kind == LocationKind.SourceFile));

        public static void GenerateStyleguideJsonFilesForControllers(
            this IEnumerable<(INamedTypeSymbol Controller, INamedTypeSymbol BlockModel)> list) =>
            list.AsParallel().ForAll(tuple =>
            {
                var targetDirectory = Directory.GetParent(tuple.Controller.GetContainingFilePath()) ?? throw new ArgumentNullException();
                var targetFileName = tuple.Controller.Name.Replace("Controller", string.Empty);

                try
                {
                    using (var fs = new FileStream($"{Path.Combine(targetDirectory.FullName, $"{targetFileName}{StyleguideJsonFileExtension}")}", FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    {
                        using (var streamWriter = new StreamWriter(fs))
                        {
                            streamWriter.Write("{ }");
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            });
    }
}