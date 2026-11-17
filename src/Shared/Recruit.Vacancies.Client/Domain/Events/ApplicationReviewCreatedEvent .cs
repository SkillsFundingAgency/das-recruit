using Esfa.Recruit.Vacancies.Client.Domain.Events.Interfaces;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events;

public class ApplicationReviewCreatedEvent : EventBase, INotification, IApplicationReviewEvent
{
    public long VacancyReference { get; set; }
}