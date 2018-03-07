using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class DeleteVacancyCommand : ICommand, IRequest
    {
        public Vacancy Vacancy { get; set; }
    }
}
