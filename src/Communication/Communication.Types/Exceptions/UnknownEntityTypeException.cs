using System;

namespace Communication.Types.Exceptions
{
    public class UnknownEntityTypeException: Exception
    {
        public UnknownEntityTypeException(string message) : base(message) { }
    }
}