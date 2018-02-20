using Esfa.Recruit.Storage.Client.Application.Commands;
using Esfa.Recruit.Storage.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Storage.Client.Application.Handlers
{
    
    public class UpdateVacancyCommandHandler: IRequestHandler<UpdateVacancyCommand>
    {
        private readonly IVacancyRepository _repository;

        public UpdateVacancyCommandHandler(IVacancyRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(UpdateVacancyCommand message, CancellationToken cancellationToken)
        {
            await _repository.UpdateAsync(message.Vacancy);
        }
    }
}
