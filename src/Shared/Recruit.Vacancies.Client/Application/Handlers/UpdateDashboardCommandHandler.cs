using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.QueryStore;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;

namespace Esfa.Recruit.Vacancies.Client.Application.Handlers
{
    public class UpdateDashboardCommandHandler : IRequestHandler<UpdateDashboardCommand>
    {
        private readonly IQueryStoreWriter _writer;
        private readonly IVacancyRepository _repository;

        public UpdateDashboardCommandHandler(IQueryStoreWriter writer, IVacancyRepository repository)
        {
            _writer = writer;
            _repository = repository;
        }

        public async Task Handle(UpdateDashboardCommand message, CancellationToken cancellationToken)
        {
            var vacancySummaries = await _repository.GetVacanciesByEmployerAccountAsync<VacancySummary>(message.EmployerAccountId);
            
            var employerVacancies = await _repository.GetVacanciesByEmployerAccountAsync(message.EmployerAccountId)
                .OrderBy(v => v.CreatedDate);
            
            await _writer.UpdateDashboardAsync(message.EmployerAccountId, vacancySummaries);
        }
    }
}