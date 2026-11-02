using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands;

public class UpdateLiveVacancyCommand : ICommand, IRequest<Unit>
{
    public Vacancy Vacancy { get; private set; }
    public VacancyUser User { get; private set; }
    public LiveUpdateKind UpdateKind { get; private set; }

    public UpdateLiveVacancyCommand(Vacancy vacancy, VacancyUser user, LiveUpdateKind updateKind)
    {
        Vacancy = vacancy;
        User = user;
        UpdateKind = updateKind;
    }
}