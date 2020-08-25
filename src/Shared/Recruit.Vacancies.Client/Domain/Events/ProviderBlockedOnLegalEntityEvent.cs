using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    public class ProviderBlockedOnLegalEntityEvent : EventBase, INotification
    {
        public long Ukprn { get; set; }
        public string EmployerAccountId { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
    }
}