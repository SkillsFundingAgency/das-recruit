using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Handlers
{
    public class CreateVacancyCommandHandler: IRequestHandler<CreateVacancyCommand>
    {
        private readonly IVacancyRepository _repository;

        public CreateVacancyCommandHandler(IVacancyRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(CreateVacancyCommand message, CancellationToken cancellationToken)
        {
            await _repository.CreateAsync(message.Vacancy);
        }
    }
}
