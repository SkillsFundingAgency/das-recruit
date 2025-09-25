using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyManage;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyView;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class VacancyManageOrchestrator(
        ILogger<VacancyManageOrchestrator> logger,
        DisplayVacancyViewModelMapper vacancyDisplayMapper,
        IRecruitVacancyClient client,
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
            var vacancy = await client.GetVacancyAsync(vrm.VacancyId.GetValueOrDefault());

            utility.CheckAuthorisedAccess(vacancy, vrm.Ukprn);

            return vacancy;
        }

        public async Task<ManageVacancyViewModel> GetManageVacancyViewModel(Vacancy vacancy,
            VacancyRouteModel vacancyRouteModel,
            int pageNumber,
            int pageSize,
            SortColumn sortColumn,
            SortOrder sortOrder,
            string locationFilter = "All")
        {
            var viewModel = new ManageVacancyViewModel
            {
                Title = vacancy.Title,
                Status = vacancy.Status,
                VacancyReference = vacancy.VacancyReference.Value.ToString(),
                Ukprn = vacancyRouteModel.Ukprn,
                VacancyId = vacancyRouteModel.VacancyId,
                ApprenticeshipType = vacancy.GetApprenticeshipType(),
            };

            viewModel.ClosingDate = viewModel.Status == VacancyStatus.Closed ? vacancy.ClosedDate?.AsGdsDate() : vacancy.ClosingDate?.AsGdsDate();
            viewModel.PossibleStartDate = vacancy.StartDate?.AsGdsDate();
            viewModel.IsDisabilityConfident = vacancy.IsDisabilityConfident;
            viewModel.IsApplyThroughFaaVacancy = vacancy.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship;
            viewModel.IsApplyThroughFatVacancy = vacancy.ApplicationMethod == ApplicationMethod.ThroughFindATraineeship;
            viewModel.CanShowEditVacancyLink = vacancy.CanExtendStartAndClosingDates;
            viewModel.CanShowCloseVacancyLink = vacancy.CanClose;
            viewModel.CanShowCloneVacancyLink = vacancy.CanClone;
            viewModel.CanShowDeleteVacancyLink = vacancy.CanDelete;
            viewModel.EmployerName = vacancy.EmployerName;

            if (vacancy.Status == VacancyStatus.Closed && vacancy.ClosureReason == ClosureReason.WithdrawnByQa)
            {
                viewModel.WithdrawnDate = vacancy.ClosedDate?.AsGdsDate();
            }

            var vacancyApplications = await client.GetVacancyApplicationsSortedAsync(vacancy.VacancyReference.Value, sortColumn, sortOrder);
            int totalUnfilteredApplicationsCount = vacancyApplications?.Count ?? 0;

            var applications = string.IsNullOrEmpty(locationFilter)
                               || locationFilter.Equals("All", StringComparison.CurrentCultureIgnoreCase)
                               || vacancyApplications.Any(fil => fil.CandidateAppliedLocations == null)
                ? vacancyApplications
                : vacancyApplications.Where(fil => fil.CandidateAppliedLocations != null 
                                                   && fil.CandidateAppliedLocations.Contains(locationFilter, StringComparison.CurrentCultureIgnoreCase))
                    .ToList();

            var pager = new PagerViewModel(
                applications?.Count ?? 0,
                pageSize,
                pageNumber,
                "Showing {0} to {1} of {2} applications",
                RouteNames.VacancyManage_Get,
                new Dictionary<string, string>
                {
                    { "locationFilter", locationFilter },
                    { "SortColumn", sortColumn.ToString() },
                    { "SortOrder", sortColumn.ToString() }
                });

            // Apply pagination: skip and take
            var pagedApplications = applications?
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            viewModel.Applications = new VacancyApplicationsViewModel
            {
                Applications = pagedApplications,
                TotalUnfilteredApplicationsCount = totalUnfilteredApplicationsCount,
                TotalFilteredApplicationsCount = applications?.Count ?? 0,
                EmploymentLocations = vacancy.EmployerLocations.GetCityDisplayList(),
                SelectedLocation = locationFilter,
                ShowDisability = vacancy.IsDisabilityConfident,
                Ukprn = vacancyRouteModel.Ukprn,
                VacancyId = vacancyRouteModel.VacancyId,
                AvailableWhere = vacancy.EmployerLocationOption,
                Pager = pager
            };

            return viewModel;
        }

        public async Task<EditVacancyViewModel> GetEditVacancyViewModel(VacancyRouteModel vrm, DateTime? proposedClosingDate, DateTime? proposedStartDate)
        {
            var vacancy = await GetVacancy(vrm);

            var viewModel = new EditVacancyViewModel
            {
                Ukprn = vrm.Ukprn,
                VacancyId = vrm.VacancyId
            };
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
                v => client.Validate(v, ValidationRules),
                v => client.UpdatePublishedVacancyAsync(vacancy, user, updateKind)
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
    }
}