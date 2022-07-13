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
        private const string StyleguideViewModelForAttributeTypeMetadataName = "Styleguide.JsonGenerator.Annotations.StyleguideViewModelForAttribute";

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
            var styleguideViewModelForAttributeType =
                compilation.GetTypeByMetadataNameOrThrow(StyleguideViewModelForAttributeTypeMetadataName);

            var blockControllers =
                allTypes.GetAllDescendantsOf(epiServerBlockControllerType).GetAllNonAbstract().GetAllFromCodeBase();
            var blockModels =
                allTypes.GetAllDescendantsOf(epiServerBlockDataType).GetAllNonAbstract().GetAllFromCodeBase(); 
            var blockViewModels = allTypes.GetAllNonAbstract().GetAllFromCodeBase()
                .GetAllWithAttribute(styleguideViewModelForAttributeType);

            var x = blockViewModels.First().GetAttributes().First().ConstructorArguments.First().Value;
            
            GroupControllersWithCorrespondingBlocks(blockControllers, blockModels)
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
