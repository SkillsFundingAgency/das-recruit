using Esfa.Recruit.Vacancies.Client.Domain.Events.Interfaces;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events;

public class ApplicationReviewWithdrawnEvent : EventBase, INotification, IApplicationReviewEvent
{
    public long VacancyReference { get; set; }
}