using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Styleguide.JsonGenerator.Extensions;


namespace Styleguide.JsonGenerator
{
    [Generator]
    public class StyleguideJsonGenerator : ISourceGenerator
    {
        private const string EPiServerBlockDataTypeMetadataName = "EPiServer.Core.BlockData";
        private const string EPiServerBlockControllerTypeMetadataName = "EPiServer.Web.Mvc.BlockController`1";

        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var compilation = (CSharpCompilation)context.Compilation;

            var allTypes = compilation.GetAllTypes();
            var epiServerBlockDataType = compilation.GetTypeByMetadataNameOrThrow(EPiServerBlockDataTypeMetadataName);
            var epiServerBlockControllerType =
                compilation.GetTypeByMetadataNameOrThrow(EPiServerBlockControllerTypeMetadataName);

            var epiServerBlockControllerDataTypes =
                allTypes.GetAllDescendantsOf(epiServerBlockControllerType).GetAllNonAbstract().GetAllFromCodeBase();
            var epiServerBlockDataTypes =
                allTypes.GetAllDescendantsOf(epiServerBlockDataType).GetAllNonAbstract().GetAllFromCodeBase();

            GroupControllersWithCorrespondingBlocks(epiServerBlockControllerDataTypes, epiServerBlockDataTypes)
                .GenerateStyleguideJsonFilesForControllers();

        }
        
        private IEnumerable<(INamedTypeSymbol Controller, INamedTypeSymbol BlockModel)> GroupControllersWithCorrespondingBlocks(IEnumerable<INamedTypeSymbol> controllers, IEnumerable<INamedTypeSymbol> blocks) => controllers
            .Join(
                blocks,
                controllerType => controllerType.BaseType?.TypeArguments.FirstOrDefault()?.MetadataName,
                blockType => blockType.MetadataName,
                (controller, blockModel) => (controller, blockModel)
            );
    }
}
