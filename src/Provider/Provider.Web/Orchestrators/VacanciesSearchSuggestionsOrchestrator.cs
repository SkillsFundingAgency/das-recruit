using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class VacanciesSearchSuggestionsOrchestrator
    {
        public const int MaxRowsInResult = 50;

        private readonly IProviderVacancyClient _providerVacancyClient;

        public VacanciesSearchSuggestionsOrchestrator(IProviderVacancyClient providerVacancyClient)
        {
            _providerVacancyClient = providerVacancyClient;
        }

        public async Task<IEnumerable<string>> GetAutoCompleteListAsync(string term, long ukprn)
        {
            if (term == null || term.Trim().Length < 3) return Enumerable.Empty<string>();

            var vacancies = await GetVacanciesAsync(ukprn);

            var data = vacancies
                .Select(v => v.Title)
                .Where(v => v.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase);
            data = data.Concat(
                vacancies
                    .Where(v => string.IsNullOrWhiteSpace(v.EmployerName) == false)
                    .Select(v => v.EmployerName)
                    .Where(v => v.Contains(term, StringComparison.OrdinalIgnoreCase))
                    .Distinct(StringComparer.OrdinalIgnoreCase));
            data = data.Concat(
                vacancies
                    .Where(v => v.VacancyReference.HasValue)
                    .Select(v => $"VAC{v.VacancyReference}")
                    .Where(v => v.StartsWith(term, StringComparison.OrdinalIgnoreCase))
                    .Distinct(StringComparer.OrdinalIgnoreCase));
            
            return data.Take(MaxRowsInResult).OrderBy(r => r);
        }

        private async Task<IEnumerable<VacancySummary>> GetVacanciesAsync(long ukprn)
        {
            var dashboard = await _providerVacancyClient.GetDashboardAsync(ukprn);

            return dashboard?.Vacancies?.OrderByDescending(v => v.CreatedDate) ?? Enumerable.Empty<VacancySummary>();
        }
    }
}