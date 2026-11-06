using System;
using Esfa.Recruit.Vacancies.Client.Domain.Events.Interfaces;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events;

public class VacancyReviewWithdrawnEvent(Guid vacancyId, long vacancyReference, Guid reviewId)
    : EventBase, INotification, IVacancyEvent, IVacancyReviewEvent
{
    public Guid VacancyId { get; } = vacancyId;

    public long VacancyReference { get;} = vacancyReference;

    public Guid ReviewId { get; } = reviewId;
}