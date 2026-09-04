using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyAnalytics;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyManage;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class VacancyManageOrchestrator(
        ILogger<VacancyManageOrchestrator> logger,
        DisplayVacancyViewModelMapper vacancyDisplayMapper,
        IRecruitVacancyClient vacancyClient,
        IUtility utility)
        : EntityValidatingOrchestrator<Vacancy, ProposedChangesEditModel>(logger)
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ClosingDate |
                                                       VacancyRuleSet.StartDate |
                                                       VacancyRuleSet.TrainingProgramme |
                                                       VacancyRuleSet.StartDateEndDate |
                                                       VacancyRuleSet.TrainingExpiryDate |
                                                       VacancyRuleSet.MinimumWage;

        public async Task<Vacancy> GetVacancy(VacancyRouteModel vrm)
        {
            var vacancy = await vacancyClient.GetVacancyAsync(vrm.VacancyId);

            utility.CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

            return vacancy;
        }

        public async Task<ManageVacancyViewModel> GetManageVacancyViewModel(
            Vacancy vacancy,
            VacancyQueryOptions queryOptions)
        {
            var vacancyReference = vacancy.VacancyReference.GetValueOrDefault();
            var isClosed = vacancy.Status == VacancyStatus.Closed;

            var (vacancyApplications, canShowArchive) = await GetApplicationsAndArchiveStatusAsync(vacancy, vacancyReference, queryOptions.SortColumn, queryOptions.SortOrder);

            if (vacancy.CanEmployerReviewApplications && vacancyApplications.Count == 0)
                throw new AuthorisationException(
                    string.Format(ExceptionMessages.UserIsNotTheOwner, OwnerType.Employer));

            var applications = ApplyFilters(vacancyApplications, queryOptions.LocationFilter, queryOptions.ApplicantFilter);

            var page = Math.Max(queryOptions.PageNumber, 1);
            var pagedApplications = applications.Skip((page - 1) * queryOptions.PageSize).Take(queryOptions.PageSize).ToList();

            return new ManageVacancyViewModel
            {
                VacancyId = vacancy.Id,
                EmployerAccountId = vacancy.EmployerAccountId,
                Title = vacancy.Title,
                Status = vacancy.Status,
                VacancyReference = vacancyReference.ToString(),
                ApprenticeshipType = vacancy.GetApprenticeshipType(),
                ClosingDate = isClosed ? vacancy.ClosedDate?.AsGdsDate() : vacancy.ClosingDate?.AsGdsDate(),
                PossibleStartDate = vacancy.StartDate?.AsGdsDate(),
                IsDisabilityConfident = vacancy.IsDisabilityConfident,
                IsApplyThroughFaaVacancy = vacancy.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship,
                TransferredProviderName = vacancy.TransferInfo?.ProviderName,
                TransferredOnDate = vacancy.TransferInfo?.TransferredDate.AsGdsDate(),
                CanShowEditVacancyLink = vacancy.CanExtendStartAndClosingDates,
                CanShowCloseVacancyLink = vacancy.CanClose,
                CanShowDeleteLink = vacancy.CanDelete,
                CanShowArchiveLink = canShowArchive,
                CanClone = vacancy.CanClone,
                IsClosedBlockedByQa = isClosed && vacancy.ClosureReason == ClosureReason.BlockedByQa,
                WithdrawnDate = isClosed && vacancy.ClosureReason == ClosureReason.WithdrawnByQa
                    ? vacancy.ClosedDate?.AsGdsDate()
                    : null,
                SelectedApplicantName = queryOptions.ApplicantFilter,
                Applications = new VacancyApplicationsViewModel
                {
                    Applications = pagedApplications,
                    TotalUnfilteredApplicationsCount = vacancyApplications.Count,
                    TotalFilteredApplicationsCount = applications.Count,
                    EmploymentLocations = vacancy.EmployerLocations.GetCityDisplayList(),
                    SelectedLocation = queryOptions.LocationFilter,
                    SelectedApplicantName = queryOptions.ApplicantFilter,
                    ShowDisability = vacancy.IsDisabilityConfident,
                    VacancyId = vacancy.Id,
                    EmployerAccountId = vacancy.EmployerAccountId,
                    VacancySharedByProvider = vacancy.CanEmployerReviewApplications,
                    AvailableWhere = vacancy.EmployerLocationOption,
                    Pager = new PagerViewModel(
                        applications.Count,
                        queryOptions.PageSize,
                        queryOptions.PageNumber,
                        "Showing {0} to {1} of {2} applications",
                        RouteNames.VacancyManage_Get,
                        new Dictionary<string, string>
                        {
                    { "locationFilter", queryOptions.LocationFilter },
                    { "SortColumn", queryOptions.SortColumn.ToString() },
                    { "SortOrder", queryOptions.SortOrder.ToString() },
                        })
                },
                TotalOutstandingApplicationsCount = applications.Count(x =>
                    x.Status == ApplicationReviewStatus.New && x.IsNotWithdrawn),
                VacancyAnalyticsViewModel = await GetVacancyAnalytics(vacancy)
            };
        }

        private async Task<(List<VacancyApplication> Applications, bool CanShowArchive)> GetApplicationsAndArchiveStatusAsync(
            Vacancy vacancy,
            long vacancyReference,
            SortColumn sortColumn,
            SortOrder sortOrder)
        {
            var applicationsTask = vacancyClient.GetVacancyApplicationsSortedAsync(
                vacancyReference, sortColumn, sortOrder, vacancy.CanEmployerReviewApplications);
            var canArchiveTask = vacancy.CanArchive
                ? utility.IsAllApplicationReviewsHasOutcomeAsync(vacancy)
                : Task.FromResult(false);

            await Task.WhenAll(applicationsTask, canArchiveTask);

            return (await applicationsTask ?? [], await canArchiveTask);
        }

        private static List<VacancyApplication> ApplyFilters(
            List<VacancyApplication> applications,
            string locationFilter,
            string applicantFilter)
        {
            IEnumerable<VacancyApplication> filtered = applications;

            if (IsActiveFilter(locationFilter))
                filtered = filtered.Where(x =>
                    x.CandidateAppliedLocations?.Contains(locationFilter) == true);

            if (IsActiveFilter(applicantFilter))
                filtered = filtered.Where(x =>
                    x.CandidateName?.Contains(applicantFilter, StringComparison.InvariantCultureIgnoreCase) == true);

            return [.. filtered];
        }

        private static bool IsActiveFilter(string filter, int minLength = 0) =>
            !string.IsNullOrEmpty(filter)
            && !filter.Equals("All", StringComparison.OrdinalIgnoreCase)
            && filter.Length > minLength;

        public async Task<EditVacancyViewModel> GetEditVacancyViewModel(VacancyRouteModel vrm, DateTime? proposedClosingDate, DateTime? proposedStartDate)
        {
            var vacancy = await GetVacancy(vrm);

            var viewModel = new EditVacancyViewModel();
            await vacancyDisplayMapper.MapFromVacancyAsync(viewModel, vacancy);

            if (proposedClosingDate.HasValue)
                viewModel.ProposedClosingDate = proposedClosingDate;

            if (proposedStartDate.HasValue)
                viewModel.ProposedStartDate = proposedStartDate;

            return viewModel;
        }

        public async Task<OrchestratorResponse> UpdatePublishedVacancyAsync(ProposedChangesEditModel m, VacancyUser user)
        {
            var vacancy = await GetVacancy(m);

            var proposedClosingDate = m.ProposedClosingDate.AsDateTimeUk()?.ToUniversalTime();
            var proposedStartDate = m.ProposedStartDate.AsDateTimeUk()?.ToUniversalTime();

            var updateKind = VacancyHelper.DetermineLiveUpdateKind(vacancy, proposedClosingDate, proposedStartDate);

            vacancy.ClosingDate = proposedClosingDate;
            vacancy.StartDate = proposedStartDate;
            
            return await ValidateAndExecute(
                vacancy, 
                v => vacancyClient.Validate(v, ValidationRules),
                v => vacancyClient.UpdatePublishedVacancyAsync(vacancy, user, updateKind)
            );
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
            var vacancyAnalyticsTask = await vacancyClient.GetVacancyAnalyticsSummaryAsync(vacancy.VacancyReference.GetValueOrDefault());
            var analyticsSummary = vacancyAnalyticsTask ?? new VacancyAnalyticsSummary();

            viewModel.AnalyticsSummary = VacancyAnalyticsSummaryMapper.MapToVacancyAnalyticsSummaryViewModel(analyticsSummary, vacancy.LiveDate.GetValueOrDefault());
            return viewModel;
        }
    }
}