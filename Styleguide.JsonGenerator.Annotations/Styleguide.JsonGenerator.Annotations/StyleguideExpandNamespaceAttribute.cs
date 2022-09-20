using System;

namespace Styleguide.JsonGenerator.Annotations
{
    // ReSharper disable once UnusedType.Global
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class StyleguideExpandNamespacesAttribute : Attribute
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public string[] ExpandableNamespaces { get; }

        public StyleguideExpandNamespacesAttribute(string[] expandableNamespaces)
        {
            ExpandableNamespaces = expandableNamespaces;
        }
    }
}