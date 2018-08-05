using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class UpdateLiveVacancyCommand : CommandBase, ICommand, IRequest
    {
        public Vacancy Vacancy { get; set; }
        public VacancyUser User { get; set; }
    }
}
