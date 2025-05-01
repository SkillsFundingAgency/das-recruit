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
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;
using Employer.Web.Configuration;
using Esfa.Recruit.Shared.Web.Helpers;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class VacancyManageOrchestrator : EntityValidatingOrchestrator<Vacancy, ProposedChangesEditModel>
    {
        private const VacancyRuleSet ValdationRules = VacancyRuleSet.ClosingDate | VacancyRuleSet.StartDate | VacancyRuleSet.TrainingProgramme | VacancyRuleSet.StartDateEndDate | VacancyRuleSet.TrainingExpiryDate | VacancyRuleSet.MinimumWage;
        private readonly DisplayVacancyViewModelMapper _vacancyDisplayMapper;
        private readonly IRecruitVacancyClient _client;
        private readonly IUtility _utility;

        public VacancyManageOrchestrator(ILogger<VacancyManageOrchestrator> logger, DisplayVacancyViewModelMapper vacancyDisplayMapper, IRecruitVacancyClient vacancyClient,  IUtility utility) : base(logger)
        {
            _vacancyDisplayMapper = vacancyDisplayMapper;
            _client = vacancyClient;
            _utility = utility;
        }

        public async Task<Vacancy> GetVacancy(VacancyRouteModel vrm, bool vacancySharedByProvider = false)
        {
            var vacancy = await _client.GetVacancyAsync(vrm.VacancyId);

            _utility.CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId, vacancySharedByProvider);

            return vacancy;
        }

        public async Task<ManageVacancyViewModel> GetManageVacancyViewModel(Vacancy vacancy, SortColumn sortColumn, SortOrder sortOrder, bool vacancySharedByProvider)
        {
            var viewModel = new ManageVacancyViewModel();

            viewModel.VacancyId = vacancy.Id;
            viewModel.EmployerAccountId = vacancy.EmployerAccountId;
            viewModel.Title = vacancy.Title;
            viewModel.Status = vacancy.Status;
            viewModel.VacancyReference = vacancy.VacancyReference.Value.ToString();
            viewModel.ClosingDate = viewModel.Status == VacancyStatus.Closed ? vacancy.ClosedDate?.AsGdsDate() : vacancy.ClosingDate?.AsGdsDate();
            viewModel.PossibleStartDate = vacancy.StartDate?.AsGdsDate();
            viewModel.IsDisabilityConfident = vacancy.IsDisabilityConfident;
            viewModel.IsApplyThroughFaaVacancy = vacancy.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship;
            viewModel.TransferredProviderName = vacancy.TransferInfo?.ProviderName;
            viewModel.TransferredOnDate = vacancy.TransferInfo?.TransferredDate.AsGdsDate();
            viewModel.CanShowEditVacancyLink = vacancy.CanExtendStartAndClosingDates;
            viewModel.CanShowCloseVacancyLink = vacancy.CanClose;
            viewModel.CanShowDeleteLink = vacancy.CanDelete;
            viewModel.IsClosedBlockedByQa = vacancy.Status == VacancyStatus.Closed && vacancy.ClosureReason == ClosureReason.BlockedByQa;
            viewModel.CanClone = vacancy.CanClone;

            if (vacancy.Status == VacancyStatus.Closed && vacancy.ClosureReason == ClosureReason.WithdrawnByQa)
            {
                viewModel.WithdrawnDate = vacancy.ClosedDate?.AsGdsDate();
            }

            var vacancyApplicationsTask = await _client.GetVacancyApplicationsSortedAsync(vacancy.VacancyReference.Value, sortColumn, sortOrder, vacancySharedByProvider);
            var applications = vacancyApplicationsTask ?? new List<VacancyApplication>();

            viewModel.Applications = new VacancyApplicationsViewModel
            {
                Applications = applications,
                ShowDisability = vacancy.IsDisabilityConfident,
                VacancyId = vacancy.Id,
                EmployerAccountId = vacancy.EmployerAccountId,
                VacancySharedByProvier = vacancySharedByProvider
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