using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class SearchHelperOrchestrator
    {
        const int maxCount = 50;
        private readonly IProviderVacancyClient _providerVacancyClient;
        public SearchHelperOrchestrator(IProviderVacancyClient providerVacancyClient)
        {
            _providerVacancyClient = providerVacancyClient;
        }
        public async Task<IEnumerable<string>> GetAutoCompleteListAsync(string term, long ukprn)
        {
            var result = new List<string>();

            if (term == null || term.Trim().Length < 3) return result;

            var vacancies = await GetVacanciesAsync(ukprn);

            var data = vacancies
                .Select(v => v.Title)
                .Where(v => v.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Take(maxCount);

            if (data.Count() < maxCount)
            {
                data = data.Concat(
                    vacancies
                        .Where(v => string.IsNullOrWhiteSpace(v.EmployerName) == false)
                        .Select(v => v.EmployerName)
                        .Where(v => v.Contains(term, StringComparison.OrdinalIgnoreCase))
                        .Take(maxCount));
            }

            if (data.Count() < maxCount)
            {
                data = data.Concat(
                    vacancies
                        .Where(v => v.VacancyReference.HasValue)
                        .Select(v => $"VAC{v.VacancyReference}")
                        .Where(v => v.StartsWith(term, StringComparison.OrdinalIgnoreCase))
                        .Take(50)
                        .OrderBy(r => r));
            }
            
            return data.Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private async Task<List<VacancySummary>> GetVacanciesAsync(long ukprn)
        {
            var dashboard = await _providerVacancyClient.GetDashboardAsync(ukprn);

            if (dashboard == null)
            {
                await _providerVacancyClient.GenerateDashboard(ukprn);
                dashboard = await _providerVacancyClient.GetDashboardAsync(ukprn);
            }

            return dashboard?.Vacancies?.OrderByDescending(v => v.CreatedDate).ToList() ?? new List<VacancySummary>();
        }

    }
}