using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands;

public class ApplicationReviewPendingUnsuccessfulFeedbackCommand : ICommand, IRequest<Unit>
{
    public VacancyUser User { get; set; }
    public ApplicationReviewStatus Status { get; set; }
    public Guid VacancyId { get; set; }
    public string Feedback { get; set; }
}