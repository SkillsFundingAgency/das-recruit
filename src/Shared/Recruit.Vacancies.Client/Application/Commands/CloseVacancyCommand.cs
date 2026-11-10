using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands;

public class CloseVacancyCommand(Guid vacancyId, VacancyUser user, ClosureReason closureReason)
    : ICommand, IRequest<Unit>
{
    public Guid VacancyId { get; } = vacancyId;
    public VacancyUser User { get; } = user;
    public ClosureReason ClosureReason { get; } = closureReason;
}