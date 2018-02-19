using Esfa.Recruit.Storage.Client.Core.Commands;
using Esfa.Recruit.Storage.Client.Core.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Storage.Client.Core.Handlers
{
    
    public class UpsertVacancyCommandHandler: IRequestHandler<UpsertVacancyCommand>
    {
        private readonly ICommandVacancyRepository _repository;

        public UpsertVacancyCommandHandler(ICommandVacancyRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(UpsertVacancyCommand message, CancellationToken cancellationToken)
        {
            await _repository.UpsertVacancyAsync(message.Patch, message.Id);
        }
    }
}
