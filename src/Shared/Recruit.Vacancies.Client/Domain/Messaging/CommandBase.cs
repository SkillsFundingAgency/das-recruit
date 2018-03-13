using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Messaging
{
    public abstract class CommandBase
    {
        protected CommandBase()
        {
            CommandId = Guid.NewGuid();
        }

        public Guid CommandId { get; }
    }
}