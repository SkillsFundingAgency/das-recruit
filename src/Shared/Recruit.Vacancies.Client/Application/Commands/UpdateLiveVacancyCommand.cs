using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands;

public class UpdateLiveVacancyCommand(Vacancy vacancy, VacancyUser user, LiveUpdateKind updateKind)
    : ICommand, IRequest<Unit>
{
    public Vacancy Vacancy { get; private set; } = vacancy;
    public VacancyUser User { get; private set; } = user;
    public LiveUpdateKind UpdateKind { get; private set; } = updateKind;
}