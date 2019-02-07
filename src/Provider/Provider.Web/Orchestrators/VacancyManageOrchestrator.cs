﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyManage;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyView;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class VacancyManageOrchestrator : EntityValidatingOrchestrator<Vacancy, ProposedChangesEditModel>
    {
        private const VacancyRuleSet ValdationRules = VacancyRuleSet.ClosingDate | VacancyRuleSet.StartDate | VacancyRuleSet.TrainingProgramme | VacancyRuleSet.StartDateEndDate | VacancyRuleSet.TrainingExpiryDate | VacancyRuleSet.MinimumWage;
        private readonly DisplayVacancyViewModelMapper _vacancyDisplayMapper;
        private readonly IProviderVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;

        public VacancyManageOrchestrator(ILogger<VacancyManageOrchestrator> logger, DisplayVacancyViewModelMapper vacancyDisplayMapper, IProviderVacancyClient client, IRecruitVacancyClient vacancyClient) : base(logger)
        {
            _vacancyDisplayMapper = vacancyDisplayMapper;
            _client = client;
            _vacancyClient = vacancyClient;
        }

        public async Task<Vacancy> GetVacancy(VacancyRouteModel vrm)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(vrm.VacancyId.GetValueOrDefault());

            Utility.CheckAuthorisedAccess(vacancy, vrm.Ukprn);

            return vacancy;
        }

        public async Task<ManageVacancyViewModel> GetManageVacancyViewModel(Vacancy vacancy)
        {
            var viewModel = new ManageVacancyViewModel();

            viewModel.Title = vacancy.Title;
            viewModel.Status = vacancy.Status;
            viewModel.VacancyReference = vacancy.VacancyReference.Value.ToString();
            viewModel.ClosingDate = viewModel.Status == VacancyStatus.Closed ? vacancy.ClosedDate?.AsGdsDate() : vacancy.ClosingDate?.AsGdsDate();
            viewModel.PossibleStartDate = vacancy.StartDate?.AsGdsDate();
            viewModel.IsDisabilityConfident = vacancy.IsDisabilityConfident;
            viewModel.IsApplyThroughFaaVacancy = vacancy.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship;
            viewModel.CanShowEditVacancyLink = vacancy.CanExtendStartAndClosingDates;
            viewModel.CanShowCloseVacancyLink = vacancy.CanClose;

            var vacancyApplicationsTask =  await _vacancyClient.GetVacancyApplicationsAsync(vacancy.VacancyReference.Value.ToString());

            var applications = vacancyApplicationsTask?.Applications ?? new List<VacancyApplication>();

            viewModel.Applications = new VacancyApplicationsViewModel
            {
                Applications = applications,
                ShowDisability = vacancy.IsDisabilityConfident
            };

            return viewModel;
        }

        public async Task<EditVacancyViewModel> GetEditVacancyViewModel(VacancyRouteModel vrm, DateTime? proposedClosingDate, DateTime? proposedStartDate)
        {
            var vacancy = await GetVacancy(vrm);

            var viewModel = new EditVacancyViewModel();
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

            vacancy.ClosingDate = m.ProposedClosingDate.AsDateTimeUk()?.ToUniversalTime();
            vacancy.StartDate = m.ProposedStartDate.AsDateTimeUk()?.ToUniversalTime();
            
            return await ValidateAndExecute(
                vacancy, 
                v => _vacancyClient.Validate(v, ValdationRules),
                v => _vacancyClient.UpdatePublishedVacancyAsync(vacancy, user)
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