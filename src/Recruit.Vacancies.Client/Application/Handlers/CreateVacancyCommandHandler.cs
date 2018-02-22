using Esfa.Recruit.Storage.Client.Application.Commands;
using Esfa.Recruit.Storage.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Storage.Client.Application.Handlers
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
