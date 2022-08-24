using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class UpdateDraftVacancyCommand : ICommand, IRequest<Unit>
    {
        public Vacancy Vacancy { get; set; }
        public VacancyUser User { get; set; }
    }
}
