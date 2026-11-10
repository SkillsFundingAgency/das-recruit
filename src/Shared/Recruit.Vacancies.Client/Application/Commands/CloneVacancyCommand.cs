using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands;

public class CloneVacancyCommand(
    Guid cloneVacancyId,
    Guid newVacancyId,
    VacancyUser user,
    SourceOrigin sourceOrigin,
    DateTime startDate,
    DateTime closingDate)
    : ICommand, IRequest<Unit>
{
    public Guid IdOfVacancyToClone { get; private set; } = cloneVacancyId;
    public Guid NewVacancyId { get; private set; } = newVacancyId;
    public VacancyUser User { get; private set; } = user;
    public SourceOrigin SourceOrigin { get; private set; } = sourceOrigin;
    public DateTime? StartDate { get; private set; } = startDate;
    public DateTime? ClosingDate { get; private set; } = closingDate;
}