using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Employer.Web.ViewModels.Vacancies;
using Esfa.Recruit.Employer.Web.ViewModels.Alerts;
using Esfa.Recruit.Shared.Web.Helpers;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class VacanciesOrchestrator(
        IEmployerVacancyClient vacancyClient)
    {
        private const int VacanciesPerPage = 25;

        public async Task<VacanciesViewModel> GetVacanciesViewModelAsync(string employerAccountId, string filter, int page, VacancyUser user, string searchTerm)
        {
            var filteringOption = SanitizeFilter(filter);
            var employerDashboard = await vacancyClient.GetDashboardAsync(employerAccountId, user.UserId, page, VacanciesPerPage, "CreatedDate", "Desc", filteringOption, searchTerm);

            int totalItems = Convert.ToInt32(employerDashboard.TotalVacancies);

            var vacancies = new List<VacancySummary>(employerDashboard.Vacancies ?? []);
            page = SanitizePage(page, totalItems);

            var vacanciesVm = vacancies
                .Select(VacancySummaryMapper.ConvertToVacancySummaryViewModel)
                .ToList();

            var alerts = new AlertsViewModel(new EmployerTransferredVacanciesAlertViewModel
            {
                TransferredVacanciesCount = employerDashboard.EmployerRevokedTransferredVacanciesAlert
                        .TransferredVacanciesCount,
                EmployerAccountId = employerAccountId,
                TransferredVacanciesProviderNames = employerDashboard.EmployerRevokedTransferredVacanciesAlert
                        .TransferredVacanciesProviderNames,

            },
                new EmployerTransferredVacanciesAlertViewModel
                {
                    EmployerAccountId = employerAccountId,
                    TransferredVacanciesCount = employerDashboard.EmployerRevokedTransferredVacanciesAlert
                        .TransferredVacanciesCount,
                    TransferredVacanciesProviderNames = employerDashboard.EmployerRevokedTransferredVacanciesAlert
                        .TransferredVacanciesProviderNames
                },
                new BlockedProviderAlertViewModel
                {
                    EmployerAccountId = employerAccountId,
                    BlockedProviderNames = employerDashboard.BlockedProviderAlert.BlockedProviderNames,
                    ClosedVacancies = employerDashboard.BlockedProviderAlert.ClosedVacancies,
                },
                new WithdrawnVacanciesAlertViewModel
                {
                    EmployerAccountId = employerAccountId,
                    ClosedVacancies = employerDashboard.WithDrawnByQaVacanciesAlert.ClosedVacancies,
                    Ukprn = user.Ukprn.GetValueOrDefault()
                }
            );

            var pager = new PagerViewModel(
                totalItems,
                VacanciesPerPage,
                page,
                "Showing {0} to {1} of {2} adverts",
                RouteNames.Vacancies_Get,
                new Dictionary<string, string>
                {
                    {"filter", filteringOption.ToString()},
                    {"searchTerm", searchTerm}
                });

            var vm = new VacanciesViewModel
            {
                EmployerAccountId = employerAccountId,
                Vacancies = vacanciesVm,
                Pager = pager,
                Filter = filteringOption,
                SearchTerm = searchTerm,
                ResultsHeading = VacancyFilterHeadingHelper.GetFilterHeading(Constants.VacancyTerm, totalItems, filteringOption, searchTerm, UserType.Employer),
                Alerts = alerts
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