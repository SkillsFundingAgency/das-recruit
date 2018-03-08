using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Mappings;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;
using Esfa.Recruit.Vacancies.Client.Domain.QueryStore;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            var employerVacancies = await _repository.GetVacanciesByEmployerAccountAsync(message.EmployerAccountId);
            var vacancySummaries = employerVacancies.Select(v => VacancySummaryMapper.MapFromVacancy(v));
            
            var dashboard = CreateDashboard(message.EmployerAccountId, vacancySummaries);
            await _writer.UpdateDashboardAsync(dashboard);
        }

        private Dashboard CreateDashboard(string employerAccountId, IEnumerable<VacancySummary> summaries)
        {
            return new Dashboard
            {
                Id = string.Format(QueryViewKeys.DashboardViewPrefix, employerAccountId),
                Vacancies = summaries
            };
        }
    }
}