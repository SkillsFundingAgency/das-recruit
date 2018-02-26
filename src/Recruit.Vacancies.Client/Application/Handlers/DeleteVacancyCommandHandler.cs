using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Handlers
{
    public class DeleteVacancyCommandHandler : IRequestHandler<DeleteVacancyCommand>
    {
        private readonly IVacancyRepository _repository;

        public DeleteVacancyCommandHandler(IVacancyRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(DeleteVacancyCommand message, CancellationToken cancellationToken)
        {
            await _repository.UpdateAsync(message.Vacancy);
        }
    }
}
