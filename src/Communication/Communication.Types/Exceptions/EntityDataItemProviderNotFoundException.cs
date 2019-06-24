using System;

namespace Communication.Types.Exceptions
{
    public class EntityDataItemProviderNotFoundException: Exception
    {
        public EntityDataItemProviderNotFoundException(string entityType) 
            : base($"Unable to resolve entity data item provider for entity type '{entityType}'") 
        { }
    }
}