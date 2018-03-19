using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class UpdateVacancyCommand : ICommand, IRequest
    {
        public Vacancy Vacancy { get; set; }

        public VacancyValidations ValidationRules { get; set; }
    }
}
