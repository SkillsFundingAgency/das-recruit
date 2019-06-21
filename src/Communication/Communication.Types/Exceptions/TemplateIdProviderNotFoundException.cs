using System;

namespace Communication.Types.Exceptions
{
    public class TemplateIdProviderNotFoundException : Exception
    {
        public TemplateIdProviderNotFoundException(string originatingServiceName) 
            : base($"There is no template registered for {originatingServiceName}") { }
    }
}