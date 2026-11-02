using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands;

public class CloseVacancyCommand : ICommand, IRequest<Unit>
{
    public Guid VacancyId { get; }
    public VacancyUser User { get; }
    public ClosureReason ClosureReason { get; }

    public CloseVacancyCommand(Guid vacancyId, VacancyUser user, ClosureReason closureReason)
    {
        VacancyId = vacancyId;
        User = user;
        ClosureReason = closureReason;
    }
}