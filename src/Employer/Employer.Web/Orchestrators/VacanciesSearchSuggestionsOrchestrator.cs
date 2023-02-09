using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class VacanciesSearchSuggestionsOrchestrator
    {
        public const int MaxRowsInResult = 50;

        private readonly IEmployerVacancyClient _employerVacancyClient;

        public VacanciesSearchSuggestionsOrchestrator(IEmployerVacancyClient employerVacancyClient)
        {
            _employerVacancyClient = employerVacancyClient;
        }

        public async Task<IEnumerable<string>> GetSearchSuggestionsAsync(string searchTerm, string employerAccountId)
        {
            if (searchTerm == null || searchTerm.Trim().Length < 3) return Enumerable.Empty<string>();

            var vacancies = (await GetVacanciesAsync(employerAccountId, searchTerm)).ToList();

            var data = vacancies.Select(c=>c.Title).
                Concat(vacancies.Select(c=>c.LegalEntityName))
                .Concat(vacancies.Select(c=>$"VAC{c.VacancyReference}"))
                .Distinct(StringComparer.OrdinalIgnoreCase);
            
            return data.Take(MaxRowsInResult).OrderBy(r => r);
        }
        
        private async Task<IEnumerable<VacancySummary>> GetVacanciesAsync(string employerAccountId, string searchTerm)
        {
            var dashboard = await _employerVacancyClient.GetDashboardAsync(employerAccountId, 1, null, searchTerm);

            return dashboard?.Vacancies?.OrderByDescending(v => v.CreatedDate) ?? Enumerable.Empty<VacancySummary>();
        }
    }
}