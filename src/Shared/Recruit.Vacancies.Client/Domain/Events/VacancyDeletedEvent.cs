using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    public class VacancyDeletedEvent : EventBase, INotification
    {
        public string EmployerAccountId { get; set; }
    }
}