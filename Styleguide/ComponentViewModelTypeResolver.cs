using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Razor;
using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;

namespace Styleguide
{
    public class ComponentViewModelTypeResolver : IComponentViewModelTypeResolver
    {
        private static string GetViewModelName(string componentName)
        {
            return $"{componentName}ViewModel";
        }

        public Type ResolveType(string componentName)
        {
            var viewModelName = GetViewModelName(componentName);
            var type = typeof(ComponentViewModelTypeResolver)
                .Assembly.GetTypes()
                .FirstOrDefault(x => x.Name.Equals(viewModelName, StringComparison.OrdinalIgnoreCase));

            if (type == null)
            {
                throw new InvalidOperationException($"Unable to find type {viewModelName}");
            }

            return type;
        }
    }

    public interface IComponentViewModelTypeResolver
    {
        Type ResolveType(string componentName);
    }

    public class RazorBasedComponentViewModelTypeResolver : IComponentViewModelTypeResolver
    {
        private readonly ControllerContext _context;

        public RazorBasedComponentViewModelTypeResolver(ControllerContext context)
        {
            _context = context;
        }

        public Type ResolveType(string componentName)
        {
            var physicalPath = ResolveViewPhysicalPath(componentName);
            var parserResults = ParseView(physicalPath);
            var viewModelName = FindViewModelName(parserResults, physicalPath);

            viewModelName = RemoveNamespace(viewModelName);

            if (viewModelName.StartsWith("IEnumerable<", StringComparison.Ordinal))
            {
                var elementTypeName = viewModelName.Substring("IEnumerable<".Length).TrimEnd('>');

                return typeof(IEnumerable<>).MakeGenericType(typeof(ComponentViewModelTypeResolver)
                    .Assembly.GetTypes()
                    .FirstOrDefault(x => x.Name.Equals(elementTypeName, StringComparison.OrdinalIgnoreCase)));
            }

            var type = typeof(ComponentViewModelTypeResolver)
                .Assembly.GetTypes()
                .FirstOrDefault(x => x.Name.Equals(viewModelName, StringComparison.OrdinalIgnoreCase));

            if (type == null)
            {
                throw new InvalidOperationException($"Unable to find type {viewModelName}");
            }

            return type;
        }

        private static ParserResults ParseView(string physicalPath)
        {
            var viewContent = File.ReadAllText(physicalPath);
            var reader = new StringReader(viewContent);
            var parserResult = new RazorParser(new CSharpCodeParser(), new HtmlMarkupParser()).Parse(reader);

            if (parserResult.Success == false)
            {
                throw new InvalidOperationException($"Unable to parse view {physicalPath}");
            }

            return parserResult;
        }

        private static string FindViewModelName(ParserResults parserResults, string physicalPath)
        {
            string viewModelName;

            if (TryFindViewModelName(parserResults.Document, out viewModelName) == false)
            {
                throw new InvalidOperationException(
                    $"Unable to find view model name in parsed view, physical path = {physicalPath}");
            }

            return viewModelName;
        }

        private string ResolveViewPhysicalPath(string componentName)
        {
            var engineResult = ViewEngines.Engines.FindPartialView(_context, componentName);

            if (engineResult.View == null ||
                engineResult.View is RazorView == false)
            {
                throw new InvalidOperationException($"Unable to find razor view for component {componentName}");
            }

            var physicalPath = System.Web.Hosting.HostingEnvironment.MapPath(((RazorView)engineResult.View).ViewPath);

            if (string.IsNullOrEmpty(physicalPath))
            {
                throw new InvalidOperationException($"Unable to map virtual path {physicalPath}");
            }

            return physicalPath;
        }

        private static bool TryFindViewModelName(SyntaxTreeNode node, out string viewModelName)
        {
            var block = node as Block;
            if (block != null)
            {
                foreach (var child in block.Children)
                {
                    if (TryFindViewModelName(child, out viewModelName))
                    {
                        return true;
                    }
                }
            }

            var span = node as Span;

            if (span != null &&
                span.Content == "model" &&
                span.Kind == SpanKind.Code)
            {
                viewModelName = string.Join(",", span.Next.Symbols
                    .SkipWhile(x => string.IsNullOrWhiteSpace(x.Content))
                    .TakeWhile(x => string.IsNullOrWhiteSpace(x.Content) == false)
                    .Select(x => x.Content));                    

                return true;
            }

            viewModelName = null;
            return false;
        }

        private static string RemoveNamespace(string viewModelName)
        {
            var indexOfLastDot = viewModelName.LastIndexOf(".", StringComparison.Ordinal);
            return indexOfLastDot > 0 ? viewModelName.Substring(indexOfLastDot) : viewModelName;
        }
    }
}