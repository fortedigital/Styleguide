using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Styleguide.JsonGenerator
{
    public abstract class JsonGeneratorBase
    {
        protected CSharpCompilation Compilation { get; }
        protected ConcurrentBag<Exception> Exceptions { get; } = new ConcurrentBag<Exception>();

        protected JsonGeneratorBase(CSharpCompilation compilation)
        {
            Compilation = compilation;
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
    }
}