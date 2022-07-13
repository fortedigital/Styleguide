using System;

namespace Styleguide.JsonGenerator.Annotations
{
    public class StyleguideViewModelForAttribute : Attribute
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public Type BlockModel { get; }
        
        public StyleguideViewModelForAttribute(Type blockModelType)
        {
            BlockModel = blockModelType;
        }
    }
}