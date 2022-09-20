using System;
using System.Collections.Generic;

namespace Styleguide.JsonGenerator.Annotations
{
    // ReSharper disable once UnusedType.Global
    [AttributeUsage(AttributeTargets.Property)]
    public class StyleguideItemsContentTypesAttribute : Attribute
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public IEnumerable<string> ContentTypeName { get; }
        
        public StyleguideItemsContentTypesAttribute(params string[] contentTypeName)
        {
            ContentTypeName = contentTypeName;
        }
    }
}
