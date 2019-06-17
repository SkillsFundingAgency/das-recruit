using System;

namespace Communication.Types.Exceptions
{
    public class TemplateIdProviderNotFoundException : Exception
    {
        public TemplateIdProviderNotFoundException(string originatingServiceName) 
            : base($"Unable to resolve template id provider for service {originatingServiceName}") 
        { }
    }
}