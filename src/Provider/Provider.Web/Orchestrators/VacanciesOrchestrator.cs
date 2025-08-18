﻿using System;
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
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class VacanciesOrchestrator
    {
        private const int VacanciesPerPage = 25;
        private readonly IProviderVacancyClient _providerVacancyClient;
        private readonly IRecruitVacancyClient _recruitVacancyClient;
        private readonly IProviderAlertsViewModelFactory _providerAlertsViewModelFactory;
        private readonly IProviderRelationshipsService _providerRelationshipsService;

        public VacanciesOrchestrator(
            IProviderVacancyClient providerVacancyClient,
            IRecruitVacancyClient recruitVacancyClient,
            IProviderAlertsViewModelFactory providerAlertsViewModelFactory,
            IProviderRelationshipsService providerRelationshipsService)
        {
            _providerVacancyClient = providerVacancyClient;
            _recruitVacancyClient = recruitVacancyClient;
            _providerAlertsViewModelFactory = providerAlertsViewModelFactory;
            _providerRelationshipsService = providerRelationshipsService;
        }

        public async Task<VacanciesViewModel> GetVacanciesViewModelAsync(
            VacancyUser user, string filter, int page, string searchTerm)
        {
            var filteringOption = SanitizeFilter(filter);
            var getDashboardTask = _providerVacancyClient.GetDashboardAsync(user.Ukprn.Value, page, filteringOption, searchTerm);
            var getUserDetailsTask = _recruitVacancyClient.GetUsersDetailsByDfEUserId(user.DfEUserId) ?? _recruitVacancyClient.GetUsersDetailsAsync(user.UserId);
            var providerTask = _providerRelationshipsService.CheckProviderHasPermissions(user.Ukprn.Value, OperationType.RecruitmentRequiresReview);
            

            await Task.WhenAll(getDashboardTask, getUserDetailsTask, providerTask);
            
            long providerVacancyCountTask = getDashboardTask.Result?.TotalVacancies 
                                            ?? await _providerVacancyClient.GetVacancyCount(user.Ukprn.Value, filteringOption, searchTerm);//TODO FAI-2541 - ignore this for ones for applications

            var dashboard = getDashboardTask.Result;
            var userDetails = getUserDetailsTask.Result;
            var providerPermissions = providerTask.Result;
            var vacancyCount = providerVacancyCountTask;
            var totalItems = Convert.ToInt32(vacancyCount);

            var alerts = _providerAlertsViewModelFactory.Create(dashboard, userDetails);

            var vacancies = new List<VacancySummary>(dashboard?.Vacancies ?? Array.Empty<VacancySummary>());
            
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
            
            var vm = new VacanciesViewModel 
            {
                Vacancies = vacanciesVm,
                Pager = pager,
                Filter = filteringOption,
                SearchTerm = searchTerm,
                ResultsHeading = VacancyFilterHeadingHelper.GetFilterHeading(Constants.VacancyTerm, totalItems, filteringOption, searchTerm, UserType.Provider),
                Alerts = alerts,
                HasEmployerReviewPermission = providerPermissions,
                Ukprn = user.Ukprn.Value,
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