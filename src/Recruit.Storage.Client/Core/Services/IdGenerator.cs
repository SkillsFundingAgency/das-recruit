using System;

namespace Esfa.Recruit.Storage.Client.Core.Services
{
    public class IdGenerator
    {
        public Guid CreateId()
        {
            return Guid.NewGuid();
        }
    }
}
