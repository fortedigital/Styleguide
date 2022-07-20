using System;

namespace Styleguide.JsonGenerator.Annotations
{
    // ReSharper disable once UnusedType.Global
    [AttributeUsage(AttributeTargets.Property)]
    public class StyleguideContentTypeAttribute : Attribute
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public string ContentTypeName { get; }
        
        public StyleguideContentTypeAttribute(string contentTypeName)
        {
            ContentTypeName = contentTypeName;
        }
    }
}