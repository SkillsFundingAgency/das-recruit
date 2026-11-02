using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands;

public class CloneVacancyCommand : ICommand, IRequest<Unit>
{
    public Guid IdOfVacancyToClone { get; private set; }
    public Guid NewVacancyId { get; private set; }
    public VacancyUser User { get; private set; }
    public SourceOrigin SourceOrigin { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? ClosingDate { get; private set; }

    public CloneVacancyCommand(
        Guid cloneVacancyId, Guid newVacancyId, VacancyUser user, 
        SourceOrigin sourceOrigin, DateTime startDate, DateTime closingDate)
    {
        IdOfVacancyToClone = cloneVacancyId;
        NewVacancyId = newVacancyId;
        User = user;
        SourceOrigin = sourceOrigin;
        StartDate = startDate;
        ClosingDate = closingDate;
    }
}