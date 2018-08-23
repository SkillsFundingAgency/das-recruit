using Esfa.Recruit.Vacancies.Client.Domain.Events.Interfaces;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    public class SetupEmployerEvent : EventBase, INotification, IEmployerEvent
    {
        public string EmployerAccountId { get; set; }
    }
}