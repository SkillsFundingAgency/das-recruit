using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Services;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Shared.Web.ViewModels.Alerts;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class VacanciesOrchestrator(
        IProviderVacancyClient providerVacancyClient,
        IProviderRelationshipsService providerRelationshipsService)
    {
        private const int VacanciesPerPage = 25;

        public async Task<VacanciesViewModel> GetVacanciesViewModelAsync(
            VacancyUser user, string filter, int page, string searchTerm)
        {
            long ukprn = user.Ukprn ?? 0;
            var filteringOption = SanitizeFilter(filter);
            var getDashboardTask = providerVacancyClient.GetDashboardAsync(ukprn, user.UserId, page, VacanciesPerPage, "CreatedDate", "Desc", filteringOption, searchTerm);

            var providerTask = providerRelationshipsService.CheckProviderHasPermissions(ukprn, OperationType.RecruitmentRequiresReview);

            await Task.WhenAll(getDashboardTask, providerTask);

            var dashboard = getDashboardTask.Result;
            bool providerPermissions = providerTask.Result;
            int totalItems = Convert.ToInt32(dashboard.TotalVacancies);

            var vacancies = new List<VacancySummary>(dashboard.Vacancies ?? []);
            
            page = SanitizePage(page, totalItems);

            var vacanciesVm = vacancies
                .Select(VacancySummaryMapper.ConvertToVacancySummaryViewModel)
                .ToList();

            var pager = new PagerViewModel(
                totalItems, 
                VacanciesPerPage,
                page, 
                "Showing {0} to {1} of {2} vacancies",
                RouteNames.Vacancies_Get,
                new Dictionary<string, string>
                {
                    {"filter", filteringOption.ToString()},
                    {"searchTerm", searchTerm}
                });

            var alerts = new AlertsViewModel(new ProviderTransferredVacanciesAlertViewModel
            {
                LegalEntityNames = dashboard.ProviderTransferredVacanciesAlert.LegalEntityNames,
                Ukprn = ukprn
            }, new WithdrawnVacanciesAlertViewModel
            {
                ClosedVacancies = dashboard.WithdrawnVacanciesAlert.ClosedVacancies,
                Ukprn = ukprn
            }, ukprn);

            var vm = new VacanciesViewModel 
            {
                Vacancies = vacanciesVm,
                Pager = pager,
                Filter = filteringOption,
                SearchTerm = searchTerm,
                ResultsHeading = VacancyFilterHeadingHelper.GetFilterHeading(Constants.VacancyTerm, totalItems, filteringOption, searchTerm, UserType.Provider),
                Alerts = alerts,
                HasEmployerReviewPermission = providerPermissions,
                Ukprn = ukprn,
                TotalVacancies = totalItems
            };

            return vm;
        }

        private int SanitizePage(int page, int totalVacancies)
        {
            return (page < 0 || page > (int)Math.Ceiling((double)totalVacancies / VacanciesPerPage)) ? 1 : page;
        }

        private FilteringOptions SanitizeFilter(string filter)
        {
            if (Enum.TryParse(typeof(FilteringOptions), filter, out var status))
                return (FilteringOptions)status;
            return FilteringOptions.All;
        }
    }
}