using System;

namespace Communication.Types.Exceptions
{
    public class InvalidEntityIdException: Exception
    {
        public InvalidEntityIdException(string entityType, string entityDataItemProviderName) 
            : base($"Unexpected entity id received by '{entityDataItemProviderName}' for type '{entityType}'") 
        { }
    }
}