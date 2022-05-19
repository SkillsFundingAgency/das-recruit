using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyManage;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyView;
using Esfa.Recruit.Shared.Web.Configuration;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class VacancyManageOrchestrator : EntityValidatingOrchestrator<Vacancy, ProposedChangesEditModel>
    {
        private const VacancyRuleSet ValdationRules = VacancyRuleSet.ClosingDate | VacancyRuleSet.StartDate | VacancyRuleSet.TrainingProgramme | VacancyRuleSet.StartDateEndDate | VacancyRuleSet.TrainingExpiryDate | VacancyRuleSet.MinimumWage;
        private readonly DisplayVacancyViewModelMapper _vacancyDisplayMapper;
        private readonly IRecruitVacancyClient _client;
        private readonly IFeature _featureToggle;
        private readonly ProviderRecruitSystemConfiguration _systemConfig;
        private readonly IUtility _utility;

        public VacancyManageOrchestrator(ILogger<VacancyManageOrchestrator> logger, DisplayVacancyViewModelMapper vacancyDisplayMapper, IRecruitVacancyClient client, IFeature featureToggle, ProviderRecruitSystemConfiguration systemConfig, IUtility utility) : base(logger)
        {
            _vacancyDisplayMapper = vacancyDisplayMapper;
            _client = client;
            _featureToggle = featureToggle;
            _systemConfig = systemConfig;
            _utility = utility;
        }

        public async Task<Vacancy> GetVacancy(VacancyRouteModel vrm)
        {
            var vacancy = await _client.GetVacancyAsync(vrm.VacancyId.GetValueOrDefault());

            _utility.CheckAuthorisedAccess(vacancy, vrm.Ukprn);

            return vacancy;
        }

        public async Task<ManageVacancyViewModel> GetManageVacancyViewModel(Vacancy vacancy,
            VacancyRouteModel vacancyRouteModel)
        {
            var viewModel = new ManageVacancyViewModel
            {
                Title = vacancy.Title,
                Status = vacancy.Status,
                VacancyReference = vacancy.VacancyReference.Value.ToString(),
                Ukprn = vacancyRouteModel.Ukprn,
                VacancyId = vacancyRouteModel.VacancyId
            };

            viewModel.ClosingDate = viewModel.Status == VacancyStatus.Closed ? vacancy.ClosedDate?.AsGdsDate() : vacancy.ClosingDate?.AsGdsDate();
            viewModel.AnalyticsAvailableAfterApprovalDate = _systemConfig.ShowAnalyticsForVacanciesApprovedAfterDate.AsGdsDate();
            viewModel.PossibleStartDate = vacancy.StartDate?.AsGdsDate();
            viewModel.IsDisabilityConfident = vacancy.IsDisabilityConfident;
            viewModel.IsApplyThroughFaaVacancy = vacancy.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship;
            viewModel.CanShowEditVacancyLink = vacancy.CanExtendStartAndClosingDates;
            viewModel.CanShowCloseVacancyLink = vacancy.CanClose;
            viewModel.CanShowCloneVacancyLink = vacancy.CanClone;
            viewModel.CanShowDeleteVacancyLink = vacancy.CanDelete;

            if (vacancy.Status == VacancyStatus.Closed && vacancy.ClosureReason == ClosureReason.WithdrawnByQa)
            {
                viewModel.WithdrawnDate = vacancy.ClosedDate?.AsGdsDate();
            }


            var applications = new List<VacancyApplication>();

            if (vacancy.LiveDate >= _systemConfig.ShowAnalyticsForVacanciesApprovedAfterDate)
            {
                var vacancyApplicationsTask = _client.GetVacancyApplicationsAsync(vacancy.VacancyReference.Value.ToString());
                var vacancyAnalyticsTask = _client.GetVacancyAnalyticsSummaryAsync(vacancy.VacancyReference.Value);

                await Task.WhenAll(vacancyApplicationsTask, vacancyAnalyticsTask);

                applications = vacancyApplicationsTask.Result?.Applications ?? new List<VacancyApplication>();
                var analyticsSummary = vacancyAnalyticsTask.Result ?? new VacancyAnalyticsSummary();
                viewModel.AnalyticsSummary = VacancyAnalyticsSummaryMapper.MapToVacancyAnalyticsSummaryViewModel(analyticsSummary, vacancy.LiveDate.GetValueOrDefault());
            }
            else
            {
                var vacancyApplications = await _client.GetVacancyApplicationsAsync(vacancy.VacancyReference.Value.ToString());
                applications = vacancyApplications?.Applications ?? new List<VacancyApplication>();
            }

            viewModel.Applications = new VacancyApplicationsViewModel
            {
                Applications = applications,
                ShowDisability = vacancy.IsDisabilityConfident,
                Ukprn = vacancyRouteModel.Ukprn,
                VacancyId = vacancyRouteModel.VacancyId
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
            await _vacancyDisplayMapper.MapFromVacancyAsync(viewModel, vacancy);

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
                v => _client.Validate(v, ValdationRules),
                v => _client.UpdatePublishedVacancyAsync(vacancy, user, updateKind)
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