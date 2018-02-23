using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Handlers
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
