using Esfa.Recruit.Vacancies.Client.Domain.Events.Interfaces;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    public class SetupProviderEvent : EventBase, INotification, IProviderEvent
    {
        public long Ukprn { get; private set; }

        public SetupProviderEvent(long ukprn)
        {
            Ukprn = ukprn;
        }
    }
}