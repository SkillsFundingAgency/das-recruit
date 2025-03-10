using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class VacanciesSearchSuggestionsOrchestrator
    {
        public const int MaxRowsInResult = 50;

        private readonly IProviderVacancyClient _providerVacancyClient;
        private readonly ServiceParameters _serviceParameters;

        public VacanciesSearchSuggestionsOrchestrator(IProviderVacancyClient providerVacancyClient, ServiceParameters serviceParameters)
        {
            _providerVacancyClient = providerVacancyClient;
            _serviceParameters = serviceParameters;
        }

        public async Task<IEnumerable<string>> GetSearchSuggestionsAsync(string searchTerm, long ukprn)
        {
            if (searchTerm == null || searchTerm.Trim().Length < 5)
            {
                return Enumerable.Empty<string>();
            }

            var vacancies = (await GetVacanciesAsync(ukprn, searchTerm)).ToList();

            var data = vacancies.Select(c=>c.Title).
                        Concat(vacancies.Select(c=>c.LegalEntityName))
                        .Concat(vacancies.Select(c=>$"VAC{c.VacancyReference}"))
                        .Distinct(StringComparer.OrdinalIgnoreCase);
            
            return data.Take(MaxRowsInResult).OrderBy(r => r);
        }
        
        private async Task<IEnumerable<VacancySummary>> GetVacanciesAsync(long ukprn, string searchTerm)
        {
            var dashboard = await _providerVacancyClient.GetDashboardAsync(ukprn, 1, null, searchTerm); 

            return dashboard?.Vacancies?.OrderByDescending(v => v.CreatedDate) ?? Enumerable.Empty<VacancySummary>();
        }
    }
}