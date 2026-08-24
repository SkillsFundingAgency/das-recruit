using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyAnalytics;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyManage;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyView;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class VacancyManageOrchestrator(ILogger<VacancyManageOrchestrator> logger,
        IRecruitVacancyClient client,
        IUtility utility)
        : EntityValidatingOrchestrator<Vacancy, ProposedChangesEditModel>(logger)
    {
        public async Task<Vacancy> GetVacancy(VacancyRouteModel vrm)
        {
            var vacancy = await client.GetVacancyAsync(vrm.VacancyId.GetValueOrDefault());

            utility.CheckAuthorisedAccess(vacancy, vrm.Ukprn);

            return vacancy;
        }

        public async Task<ManageVacancyViewModel> GetManageVacancyViewModel(
            Vacancy vacancy,
            VacancyRouteModel vacancyRouteModel,
            int pageNumber,
            int pageSize,
            SortColumn sortColumn,
            SortOrder sortOrder,
            string locationFilter = "All",
            string applicantFilter = "")
        {
            var vacancyReference = vacancy.VacancyReference.GetValueOrDefault();
            var isClosed = vacancy.Status == VacancyStatus.Closed;

            var applicationsTask = client.GetVacancyApplicationsSortedAsync(vacancyReference, sortColumn, sortOrder);
            var canArchiveTask = vacancy.CanArchive
                ? utility.IsAllApplicationReviewsHasOutcomeAsync(vacancy)
                : Task.FromResult(false);

            (List<VacancyApplication> vacancyApplications, var canShowArchive) = (await applicationsTask ?? [], await canArchiveTask);

            IEnumerable<VacancyApplication> filtered = vacancyApplications;

            if (IsActiveFilter(locationFilter))
                filtered = filtered.Where(x =>
                    x.CandidateAppliedLocations?.Contains(locationFilter) == true);

            if (IsActiveFilter(applicantFilter, minLength: 3))
                filtered = filtered.Where(x =>
                    x.CandidateName?.Contains(applicantFilter, StringComparison.InvariantCultureIgnoreCase) == true);

            var applications = filtered.ToList();

            var page = Math.Max(pageNumber, 1);
            var pagedApplications = applications
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new ManageVacancyViewModel
            {
                Title = vacancy.Title,
                Status = vacancy.Status,
                VacancyReference = vacancyReference.ToString(),
                Ukprn = vacancyRouteModel.Ukprn,
                VacancyId = vacancyRouteModel.VacancyId,
                ApprenticeshipType = vacancy.GetApprenticeshipType(),
                ClosingDate = isClosed ? vacancy.ClosedDate?.AsGdsDate() : vacancy.ClosingDate?.AsGdsDate(),
                PossibleStartDate = vacancy.StartDate?.AsGdsDate(),
                IsDisabilityConfident = vacancy.IsDisabilityConfident,
                IsApplyThroughFaaVacancy = vacancy.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship,
                IsApplyThroughFatVacancy = vacancy.ApplicationMethod == ApplicationMethod.ThroughFindATraineeship,
                CanShowEditVacancyLink = vacancy.CanExtendStartAndClosingDates,
                CanShowCloseVacancyLink = vacancy.CanClose,
                CanShowCloneVacancyLink = vacancy.CanClone,
                CanShowDeleteVacancyLink = vacancy.CanDelete,
                CanShowArchiveVacancyLink = canShowArchive,
                EmployerName = vacancy.EmployerName,
                WithdrawnDate = isClosed && vacancy.ClosureReason == ClosureReason.WithdrawnByQa
                    ? vacancy.ClosedDate?.AsGdsDate()
                    : null,
                SelectedApplicantName = applicantFilter,
                Applications = new VacancyApplicationsViewModel
                {
                    Applications = pagedApplications,
                    TotalUnfilteredApplicationsCount = vacancyApplications.Count,
                    TotalFilteredApplicationsCount = applications.Count,
                    EmploymentLocations = vacancy.EmployerLocations.GetCityDisplayList(),
                    SelectedLocation = locationFilter,
                    SelectedApplicantName = applicantFilter,
                    ShowDisability = vacancy.IsDisabilityConfident,
                    Ukprn = vacancyRouteModel.Ukprn,
                    VacancyId = vacancyRouteModel.VacancyId,
                    AvailableWhere = vacancy.EmployerLocationOption,
                    Pager = new PagerViewModel(
                        applications.Count,
                        pageSize,
                        page,
                        "Showing {0} to {1} of {2} applications",
                        RouteNames.VacancyManage_Get,
                        new Dictionary<string, string>
                        {
                    { "locationFilter", locationFilter },
                    { "SortColumn", sortColumn.ToString() },
                    { "SortOrder", sortOrder.ToString() },
                        })
                },
                TotalOutstandingApplicationsCount = applications.Count(x =>
                    x.Status == ApplicationReviewStatus.New && x.IsNotWithdrawn),
                VacancyAnalyticsViewModel = await GetVacancyAnalytics(vacancy)
            };

            static bool IsActiveFilter(string filter, int minLength = 0) =>
                !string.IsNullOrEmpty(filter)
                && !filter.Equals("All", StringComparison.OrdinalIgnoreCase)
                && filter.Length > minLength;
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, ProposedChangesEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, ProposedChangesEditModel>
            {
                { e => e.StartDate, vm => vm.ProposedStartDate },
                { e => e.ClosingDate, vm => vm.ProposedClosingDate }
            };

            return mappings;
        }

        private async Task<VacancyAnalyticsViewModel> GetVacancyAnalytics(Vacancy vacancy)
        {
            var viewModel = new VacancyAnalyticsViewModel();
            var vacancyAnalyticsTask = await client.GetVacancyAnalyticsSummaryAsync(vacancy.VacancyReference.GetValueOrDefault());
            var analyticsSummary = vacancyAnalyticsTask ?? new VacancyAnalyticsSummary();
            
            viewModel.AnalyticsSummary = VacancyAnalyticsSummaryMapper.MapToVacancyAnalyticsSummaryViewModel(analyticsSummary, vacancy.LiveDate.GetValueOrDefault());

            return viewModel;
        }
    }
}