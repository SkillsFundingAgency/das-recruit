using System;

namespace Esfa.Recruit.Storage.Client.Application.Services
{
    public class IdGenerator
    {
        public Guid CreateId()
        {
            return Guid.NewGuid();
        }
    }
}
