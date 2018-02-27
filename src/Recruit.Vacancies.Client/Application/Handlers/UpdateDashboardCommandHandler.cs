using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Mappings;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;
using Esfa.Recruit.Vacancies.Client.Domain.QueryStore;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Handlers
{
    public class UpdateDashboardCommandHandler : IRequestHandler<UpdateDashboardCommand>
    {
        private readonly IQueryStoreReader _reader;
        private readonly IQueryStoreWriter _writer;

        public UpdateDashboardCommandHandler(IQueryStoreReader reader, IQueryStoreWriter repository)
        {
            _reader = reader;
            _writer = repository;
        }

        public async Task Handle(UpdateDashboardCommand message, CancellationToken cancellationToken)
        {
            var key = message.Vacancy.EmployerAccountId;
            var updatedVacancy = message.Vacancy;
            var dashboard = await _reader.GetDashboardAsync(key);

            var updatedVacancySummary = VacancySummaryMapper.MapFromVacancy(updatedVacancy);

            if (dashboard == null)
            {
                var newDashboard = CreateNewDashboard(key, updatedVacancySummary);
                await _writer.UpdateDashboardAsync(key, newDashboard);
            }
            else
            {
                dashboard.Vacancies = dashboard.Vacancies
                                                .Where(v => v.Id != updatedVacancy.Id)
                                                .Concat(new [] { updatedVacancySummary });
                await _writer.UpdateDashboardAsync(key, dashboard);
            }
        }

        private Dashboard CreateNewDashboard(string key, VacancySummary updatedVacancySummary)
        {
            return new Dashboard
            {
                EmployerAccountId = key,
                Id = Guid.NewGuid(),
                Vacancies = new List<VacancySummary>
                {
                    updatedVacancySummary
                }
            };
        }
    }
}
