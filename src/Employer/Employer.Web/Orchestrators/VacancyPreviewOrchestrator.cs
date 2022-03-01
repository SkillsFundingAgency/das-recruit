using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Preview;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ErrMsg = Esfa.Recruit.Shared.Web.ViewModels.ErrorMessages;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class VacancyPreviewOrchestrator : EntityValidatingOrchestrator<Vacancy, VacancyPreviewViewModel>
    {
        private const VacancyRuleSet SubmitValidationRules = VacancyRuleSet.All;
        private const VacancyRuleSet RejectValidationRules = VacancyRuleSet.None;
        private const VacancyRuleSet SoftValidationRules = VacancyRuleSet.MinimumWage | VacancyRuleSet.TrainingExpiryDate;

        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly DisplayVacancyViewModelMapper _vacancyDisplayMapper;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly ILegalEntityAgreementService _legalEntityAgreementService;
        private readonly IMessaging _messaging;
        private readonly IUtility _utility;
        private readonly ExternalLinksConfiguration _externalLinksConfiguration;

        public VacancyPreviewOrchestrator(
            IRecruitVacancyClient vacancyClient,
            ILogger<VacancyPreviewOrchestrator> logger,
            DisplayVacancyViewModelMapper vacancyDisplayMapper, 
            IReviewSummaryService reviewSummaryService, 
            ILegalEntityAgreementService legalEntityAgreementService,
            IMessaging messaging,
            IOptions<ExternalLinksConfiguration> externalLinksOptions,
            IUtility utility) : base(logger)
        {
            _vacancyClient = vacancyClient;
            _vacancyDisplayMapper = vacancyDisplayMapper;
            _reviewSummaryService = reviewSummaryService;
            _legalEntityAgreementService = legalEntityAgreementService;
            _messaging = messaging;
            _utility = utility;
            _externalLinksConfiguration = externalLinksOptions.Value;
        }

        public async Task<VacancyPreviewViewModel> GetVacancyPreviewViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancyTask = _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Vacancy_Preview_Get);
            var programmesTask = _vacancyClient.GetActiveApprenticeshipProgrammesAsync();

            await Task.WhenAll(vacancyTask, programmesTask);

            var vacancy = vacancyTask.Result;
            var programme = programmesTask.Result.SingleOrDefault(p => p.Id == vacancy.ProgrammeId);

            var vm = new VacancyPreviewViewModel();
            await _vacancyDisplayMapper.MapFromVacancyAsync(vm, vacancy);

            vm.RejectedReason = vacancy.EmployerRejectedReason;
            vm.HasProgramme = vacancy.ProgrammeId != null;
            vm.HasWage = vacancy.Wage != null;
            vm.CanShowReference = vacancy.Status != VacancyStatus.Draft;
            vm.CanShowDraftHeader = vacancy.Status == VacancyStatus.Draft;
            vm.SoftValidationErrors = GetSoftValidationErrors(vacancy);
            vm.EducationLevelName =
                EducationLevelNumberHelper.GetEducationLevelNameOrDefault(programme.EducationLevelNumber, programme.ApprenticeshipLevel);

            if (programme != null) vm.ApprenticeshipLevel = programme.ApprenticeshipLevel;

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value, 
                    ReviewFieldMappingLookups.GetPreviewReviewFieldIndicators());
            }
            
            return vm;
        }
        
        public async Task<OrchestratorResponse<SubmitVacancyResponse>> SubmitVacancyAsync(SubmitEditModel m, VacancyUser user)
        {
            var vacancy = await _utility.GetAuthorisedVacancyAsync(m, RouteNames.Preview_Submit_Post);
            
            if (!vacancy.CanSubmit)
                throw new InvalidStateException(string.Format(ErrMsg.VacancyNotAvailableForEditing, vacancy.Title));

            var employerDescriptionTask = _vacancyClient.GetEmployerDescriptionAsync(vacancy);
            var employerNameTask = _vacancyClient.GetEmployerNameAsync(vacancy);
            
            await Task.WhenAll(employerDescriptionTask, employerNameTask);

            vacancy.EmployerDescription = employerDescriptionTask.Result;
            vacancy.EmployerName = employerNameTask.Result;

            return await ValidateAndExecute(
                vacancy,
                v => ValidateVacancy(v, SubmitValidationRules),
                v => SubmitActionAsync(v, user)
                );
        } 

        private EntityValidationResult ValidateVacancy(Vacancy vacancy, VacancyRuleSet rules)
        {
            var result = _vacancyClient.Validate(vacancy, rules);
            FlattenErrors(result.Errors);
            return result;
        }

        private async Task<SubmitVacancyResponse> SubmitActionAsync(Vacancy vacancy, VacancyUser user)
        {
            var response = new SubmitVacancyResponse
            {
                HasLegalEntityAgreement = await _legalEntityAgreementService.HasLegalEntityAgreementAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId),
                IsSubmitted = false
            };

            if (response.HasLegalEntityAgreement == false)
                return response;

            var command = new SubmitVacancyCommand(vacancy.Id, user,OwnerType.Employer, vacancy.EmployerDescription);

            await _messaging.SendCommandAsync(command);

            response.IsSubmitted = true;

            return response;
        }


        private async Task<RejectVacancyResponse> RejectActionAsync(Vacancy vacancy, VacancyUser user)
        {
            var command = new RejectVacancyCommand { VacancyReference = (long)vacancy.VacancyReference };

            await _messaging.SendCommandAsync(command);

            return new  RejectVacancyResponse { IsRejected = true };
        }

        public async Task ClearRejectedVacancyReason(SubmitReviewModel m, VacancyUser user)
        {
            var vacancy = await _utility.GetAuthorisedVacancyAsync(m, RouteNames.ApproveJobAdvert_Post);

            vacancy.EmployerRejectedReason = null;

            await _vacancyClient.UpdateDraftVacancyAsync(vacancy, user);
        }

        public async Task UpdateRejectedVacancyReason(SubmitReviewModel m, VacancyUser user)
        {
            var vacancy = await _utility.GetAuthorisedVacancyAsync(m, RouteNames.ApproveJobAdvert_Post);

            vacancy.EmployerRejectedReason = m.RejectedReason;

            await _vacancyClient.UpdateDraftVacancyAsync(vacancy, user);
        }

        public async Task<OrchestratorResponse<SubmitVacancyResponse>> ApproveJobAdvertAsync(ApproveJobAdvertViewModel m, VacancyUser user)
        {
            var vacancy = await _utility.GetAuthorisedVacancyAsync(m, RouteNames.ApproveJobAdvert_Post);

            if (!vacancy.CanReview)
                throw new InvalidStateException(string.Format(ErrMsg.VacancyNotAvailableForEditing, vacancy.Title));

            var employerDescriptionTask = _vacancyClient.GetEmployerDescriptionAsync(vacancy);
            var employerNameTask = _vacancyClient.GetEmployerNameAsync(vacancy);

            await Task.WhenAll(employerDescriptionTask, employerNameTask);

            vacancy.EmployerDescription = employerDescriptionTask.Result;
            vacancy.EmployerName = employerNameTask.Result;

            return await ValidateAndExecute(
                vacancy,
                v => ValidateVacancy(v, SubmitValidationRules),
                v => SubmitActionAsync(v, user)
                );
        }

        public async Task<OrchestratorResponse<RejectVacancyResponse>> RejectJobAdvertAsync(RejectJobAdvertViewModel vm, VacancyUser user)
        {
            var vacancy = await _utility.GetAuthorisedVacancyAsync(vm, RouteNames.RejectJobAdvert_Post);

            if (!vacancy.CanReject)
                throw new InvalidStateException(string.Format(ErrMsg.VacancyNotAvailableForReject, vacancy.Title));
            
            return await ValidateAndExecute(
               vacancy,
               v => ValidateVacancy(v, RejectValidationRules),
               v => RejectActionAsync(v, user)
               );
        }

        public async Task<JobAdvertConfirmationViewModel> GetVacancyConfirmationJobAdvertAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(vrm.VacancyId);            

            var vm = new JobAdvertConfirmationViewModel
            {                 
                Title = vacancy.Title,
                VacancyReference = vacancy.VacancyReference?.ToString(),
                ApprovedJobAdvert = vacancy.Status == VacancyStatus.Submitted,
                RejectedJobAdvert = vacancy.Status == VacancyStatus.Rejected,
                TrainingProviderName = vacancy.TrainingProvider.Name,
                FindAnApprenticeshipUrl = _externalLinksConfiguration.FindAnApprenticeshipUrl
            };        

            return vm;
        }

        public async Task<RejectJobAdvertViewModel> GetVacancyRejectJobAdvertAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(vrm.VacancyId);

            var vm = new RejectJobAdvertViewModel
            {
                RejectionReason = vacancy.EmployerRejectedReason,
                TrainingProviderName = vacancy.TrainingProvider.Name
            };

            return vm;
        }

        public async Task<OrchestratorResponse<TaskListViewModel>> GetEmployerTaskList(VacancyRouteModel vrm)
        {
            return new OrchestratorResponse<TaskListViewModel>(new TaskListViewModel());
        }

        private void FlattenErrors(IList<EntityValidationError> errors)
        {
            //Flatten Qualification errors to its ViewModel parent instead. 'Qualifications[1].Grade' becomes 'Qualifications'
            foreach (var error in errors.Where(e => e.PropertyName.StartsWith(nameof(Vacancy.Qualifications))))
            {
                error.PropertyName = nameof(VacancyPreviewViewModel.Qualifications);
            }
        }

        private EntityValidationResult GetSoftValidationErrors(Vacancy vacancy)
        {
            var result = ValidateVacancy(vacancy, SoftValidationRules);
            MapValidationPropertiesToViewModel(result);
            return result;
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, VacancyPreviewViewModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, VacancyPreviewViewModel>();

            mappings.Add(e => e.ShortDescription, vm => vm.ShortDescription);
            mappings.Add(e => e.ClosingDate, vm => vm.ClosingDate);
            mappings.Add(e => e.Wage, vm => vm.HasWage);
            mappings.Add(e => e.Wage.FixedWageYearlyAmount, vm => vm.WageText);
            mappings.Add(e => e.Wage.WeeklyHours, vm => vm.HoursPerWeek);
            mappings.Add(e => e.Wage.WorkingWeekDescription, vm => vm.WorkingWeekDescription);
            mappings.Add(e => e.Wage.WageType, vm => vm.WageText);
            mappings.Add(e => e.Wage.Duration, vm => vm.ExpectedDuration);
            mappings.Add(e => e.Wage.DurationUnit, vm => vm.ExpectedDuration);
            mappings.Add(e => e.StartDate, vm => vm.PossibleStartDate);
            mappings.Add(e => e.ProgrammeId, vm => vm.HasProgramme);
            mappings.Add(e => e.NumberOfPositions, vm => vm.NumberOfPositions);
            mappings.Add(e => e.Description, vm => vm.VacancyDescription);
            mappings.Add(e => e.TrainingDescription, vm => vm.TrainingDescription);
            mappings.Add(e => e.OutcomeDescription, vm => vm.OutcomeDescription);
            mappings.Add(e => e.Skills, vm => vm.Skills);
            mappings.Add(e => e.Qualifications, vm => vm.Qualifications);
            mappings.Add(e => e.ThingsToConsider, vm => vm.ThingsToConsider);
            mappings.Add(e => e.EmployerName, vm => vm.EmployerName);
            mappings.Add(e => e.EmployerDescription, vm => vm.EmployerDescription);
            mappings.Add(e => e.EmployerWebsiteUrl, vm => vm.EmployerWebsiteUrl);
            mappings.Add(e => e.EmployerContact.Name, vm => vm.EmployerContactName);
            mappings.Add(e => e.EmployerContact.Email, vm => vm.EmployerContactEmail);
            mappings.Add(e => e.EmployerContact.Phone, vm => vm.EmployerContactTelephone);
            mappings.Add(e => e.EmployerLocation, vm => vm.EmployerAddressElements);
            mappings.Add(e => e.EmployerLocation.AddressLine1, vm => vm.EmployerAddressElements);
            mappings.Add(e => e.EmployerLocation.AddressLine2, vm => vm.EmployerAddressElements);
            mappings.Add(e => e.EmployerLocation.AddressLine3, vm => vm.EmployerAddressElements);
            mappings.Add(e => e.EmployerLocation.AddressLine4, vm => vm.EmployerAddressElements);
            mappings.Add(e => e.EmployerLocation.Postcode, vm => vm.EmployerAddressElements);
            mappings.Add(e => e.EmployerLocation.Latitude, vm => vm.EmployerAddressElements);
            mappings.Add(e => e.EmployerLocation.Longitude, vm => vm.EmployerAddressElements);
            mappings.Add(e => e.ApplicationInstructions, vm => vm.ApplicationInstructions);
            mappings.Add(e => e.ApplicationUrl, vm => vm.ApplicationUrl);
            mappings.Add(e => e.TrainingProvider, vm => vm.ProviderName);
            mappings.Add(e => e.TrainingProvider.Ukprn, vm => vm.ProviderName);

            return mappings;
        }
    }
}
