using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyManage;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyAnalytics;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;

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
            int pageNumber,
            int pageSize,
            SortColumn sortColumn,
            SortOrder sortOrder,
            string locationFilter = "All")
        {
            var vacancyReference = vacancy.VacancyReference.GetValueOrDefault();
            var isClosed = vacancy.Status == VacancyStatus.Closed;

            var applicationsTask = vacancyClient.GetVacancyApplicationsSortedAsync(
                vacancyReference, sortColumn, sortOrder, vacancy.CanEmployerReviewApplications);
            var canArchiveTask = vacancy.CanArchive
                ? utility.IsAllApplicationReviewsHasOutcomeAsync(vacancy)
                : Task.FromResult(false);

            // WhenAll so both are observed even if one faults.
            await Task.WhenAll(applicationsTask, canArchiveTask);

            var vacancyApplications = await applicationsTask ?? [];
            var canShowArchive = await canArchiveTask;

            if (vacancy.CanEmployerReviewApplications && vacancyApplications.Count == 0)
            {
                // If there are no applications the employer user shouldn't be here.
                throw new AuthorisationException(
                    string.Format(ExceptionMessages.UserIsNotTheOwner, OwnerType.Employer));
            }

            var applyLocationFilter =
                !string.IsNullOrEmpty(locationFilter)
                && !locationFilter.Equals("All", StringComparison.OrdinalIgnoreCase)
                && vacancyApplications.All(x => x.CandidateAppliedLocations is not null);

            var applications = applyLocationFilter
                ? vacancyApplications.Where(x => x.CandidateAppliedLocations!.Contains(locationFilter)).ToList()
                : vacancyApplications;

            var page = Math.Max(pageNumber, 1);
            var filteredCount = applications.Count;

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
                Applications = new VacancyApplicationsViewModel
                {
                    Applications = applications.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                    TotalUnfilteredApplicationsCount = vacancyApplications.Count,
                    TotalFilteredApplicationsCount = filteredCount,
                    EmploymentLocations = vacancy.EmployerLocations.GetCityDisplayList(),
                    SelectedLocation = locationFilter,
                    ShowDisability = vacancy.IsDisabilityConfident,
                    VacancyId = vacancy.Id,
                    EmployerAccountId = vacancy.EmployerAccountId,
                    VacancySharedByProvider = vacancy.CanEmployerReviewApplications,
                    AvailableWhere = vacancy.EmployerLocationOption,
                    Pager = new PagerViewModel(
                        filteredCount,
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
                TotalOutstandingApplicationsCount = applications.Count(x => x.Status == ApplicationReviewStatus.New && x.IsNotWithdrawn),
                VacancyAnalyticsViewModel = await GetVacancyAnalytics(vacancy)
            };
        }

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