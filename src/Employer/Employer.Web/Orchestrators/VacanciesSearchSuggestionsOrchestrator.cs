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

            var vacancies = await GetVacanciesAsync(employerAccountId);

            var data = GetVacanciesPartialMatchingTitleSuggestions(searchTerm, vacancies)
                        .Concat(GetVacanciesPartialMatchingEmployerNameSuggestions(searchTerm, vacancies))
                        .Concat(GetVacanciesPartialMatchingVacancyReferenceSuggestions(searchTerm, vacancies));
            
            return data.Take(MaxRowsInResult).OrderBy(r => r);
        }
        
        private IEnumerable<string> GetVacanciesPartialMatchingTitleSuggestions(string searchTerm, IEnumerable<VacancySummary> vacancies)
        {
            return vacancies
                .Select(v => v.Title)
                .Where(v => v.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private IEnumerable<string> GetVacanciesPartialMatchingEmployerNameSuggestions(string searchTerm, IEnumerable<VacancySummary> vacancies)
        {
            return vacancies
                .Where(v => string.IsNullOrWhiteSpace(v.LegalEntityName) == false)
                .Select(v => v.LegalEntityName)
                .Where(v => v.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private IEnumerable<string> GetVacanciesPartialMatchingVacancyReferenceSuggestions(string searchTerm, IEnumerable<VacancySummary> vacancies)
        {
            return vacancies
                .Where(v => v.VacancyReference.HasValue)
                .Select(v => $"VAC{v.VacancyReference}")
                .Where(v => v.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private async Task<IEnumerable<VacancySummary>> GetVacanciesAsync(string employerAccountId)
        {
            var dashboard = await _employerVacancyClient.GetDashboardAsync(employerAccountId);

            return dashboard?.Vacancies?.OrderByDescending(v => v.CreatedDate) ?? Enumerable.Empty<VacancySummary>();
        }
    }
}