using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using MediatR;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands;

public class ApplicationReviewStatusEditCommand : ICommand, IRequest<bool>
{
    public Guid ApplicationReviewId { get; set; }
    public ApplicationReviewStatus? Outcome { get; set; }
    public string? CandidateFeedback { get; set; }
    public VacancyUser User { get; set; }
}