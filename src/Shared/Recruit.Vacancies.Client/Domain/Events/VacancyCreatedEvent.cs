using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    public class VacancyCreatedEvent : EventBase, INotification
    {
        public string EmployerAccountId { get; set; }
    }
}