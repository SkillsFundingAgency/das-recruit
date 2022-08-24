using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
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
            if (searchTerm == null || searchTerm.Trim().Length < 3) return Enumerable.Empty<string>();

            var vacancies = await GetVacanciesAsync(ukprn);

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

        private async Task<IEnumerable<VacancySummary>> GetVacanciesAsync(long ukprn)
        {
            var dashboard = await _providerVacancyClient.GetDashboardAsync(ukprn, _serviceParameters.VacancyType.GetValueOrDefault());

            return dashboard?.Vacancies?.OrderByDescending(v => v.CreatedDate) ?? Enumerable.Empty<VacancySummary>();
        }
    }
}