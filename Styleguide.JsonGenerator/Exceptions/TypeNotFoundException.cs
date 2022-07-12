using System;

namespace Styleguide.JsonGenerator.Exceptions
{
    public class TypeNotFoundException : Exception
    {
        public TypeNotFoundException()
        {
        }

        public TypeNotFoundException(string message) : base(message)
        {
        }
    }
}