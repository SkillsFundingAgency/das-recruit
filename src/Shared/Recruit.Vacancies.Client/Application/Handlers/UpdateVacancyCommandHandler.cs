using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Handlers
{
    public class UpdateVacancyCommandHandler : IRequestHandler<UpdateVacancyCommand>
    {
        private readonly IVacancyRepository _repository;
        public readonly IVacancyValidator _validator;

        public UpdateVacancyCommandHandler(IVacancyRepository repository, IVacancyValidator validator)
        {
            _validator = validator;
            _repository = repository;
        }

        public async Task Handle(UpdateVacancyCommand message, CancellationToken cancellationToken)
        {
            _validator.ValidateAndThrow(message.Vacancy, message.ValidationRules);

            await _repository.UpdateAsync(message.Vacancy);
        }
    }
}
