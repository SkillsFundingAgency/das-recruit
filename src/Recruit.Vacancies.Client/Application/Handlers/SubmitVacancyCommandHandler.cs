using Esfa.Recruit.Storage.Client.Core.Commands;
using Esfa.Recruit.Storage.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Storage.Client.Core.Handlers
{
    public class SubmitVacancyCommandHandler : IRequestHandler<SubmitVacancyCommand>
    {
        private readonly IVacancyRepository _repository;

        public SubmitVacancyCommandHandler(IVacancyRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(SubmitVacancyCommand message, CancellationToken cancellationToken)
        {
            await _repository.UpdateAsync(message.Vacancy);
        }
    }
}
