using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Styleguide.JsonGenerator.Extensions;

namespace Styleguide.JsonGenerator
{
    public abstract class JsonGeneratorBase
    {
        protected CSharpCompilation Compilation { get; }
        protected IEnumerable<string> ExpandableNamespaces { get; }
        protected ConcurrentBag<Exception> Exceptions { get; } = new ConcurrentBag<Exception>();

        protected JsonGeneratorBase(CSharpCompilation compilation)
        {
            Compilation = compilation;

            ExpandableNamespaces = GetExpandableAssemblies();
        }

        protected virtual void Setup()
        {
            Debug.WriteLine("Before generation");    
        }

        protected abstract IEnumerable<GenerationSource> GetSourceFiles();
        protected abstract void GenerateJsonFiles(IEnumerable<GenerationSource> sourceFiles);

        protected virtual void CleanUp()
        {
            if (!Exceptions.IsEmpty)
            {
                throw new AggregateException("Generation thrown several exceptions", Exceptions);
            }
        }

        public void Run()
        {
            Setup();
            var sources = GetSourceFiles();
            GenerateJsonFiles(sources);
            CleanUp();
        }

        private IEnumerable<string> GetExpandableAssemblies()
        {
            var configurationBaseClassType =
                Compilation.GetTypeByMetadataNameOrThrow(
                    "Styleguide.JsonGenerator.Annotations.StyleguideConfigurationBase");

            var extendableAssembliesAttributeType =
                Compilation.GetTypeByMetadataNameOrThrow(
                    "Styleguide.JsonGenerator.Annotations.StyleguideExpandNamespacesAttribute");
            
            var configurationClass = Compilation.GetAllTypes().GetAllDescendantsOf(configurationBaseClassType).FirstOrDefault();

            return configurationClass?
                    .GetAttributes()
                    .FirstOrDefault(attribute =>
                        SymbolEqualityComparer.Default.Equals(attribute.AttributeClass,
                            extendableAssembliesAttributeType))?
                    .ConstructorArguments
                    .Take(1)
                    .Select(arg => arg.Values)
                    .FirstOrDefault()
                    .Select(x => (string)x.Value)
                    .ToArray() 
                   ?? Array.Empty<string>();
        }
    }
}