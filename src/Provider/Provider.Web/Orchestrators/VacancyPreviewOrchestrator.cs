using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.Models;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using Microsoft.Extensions.Logging;
using ErrMsg = Esfa.Recruit.Shared.Web.ViewModels.ErrorMessages;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class VacancyPreviewOrchestrator : EntityValidatingOrchestrator<Vacancy, VacancyPreviewViewModel>
    {
        private const VacancyRuleSet SubmitValidationRules = VacancyRuleSet.All;
        private const VacancyRuleSet SoftValidationRules = VacancyRuleSet.MinimumWage | VacancyRuleSet.TrainingExpiryDate;

        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly DisplayVacancyViewModelMapper _vacancyDisplayMapper;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly IProviderRelationshipsService _providerRelationshipsService;
        private readonly ILegalEntityAgreementService _legalEntityAgreementService;
        private readonly ITrainingProviderAgreementService _trainingProviderAgreementService;
        private readonly IMessaging _messaging;
        private readonly IUtility _utility;
        private readonly ServiceParameters _serviceParameters;
        private readonly IFeature _feature;

        public VacancyPreviewOrchestrator(IRecruitVacancyClient vacancyClient,
            ILogger<VacancyPreviewOrchestrator> logger,
            DisplayVacancyViewModelMapper vacancyDisplayMapper, 
            IReviewSummaryService reviewSummaryService,
            IProviderRelationshipsService providerRelationshipsService,
            ILegalEntityAgreementService legalEntityAgreementService,
            ITrainingProviderAgreementService trainingProviderAgreementService,
            IMessaging messaging,
            IUtility utility,
            ServiceParameters serviceParameters,
            IFeature feature) : base(logger)
        {
            _vacancyClient = vacancyClient;
            _vacancyDisplayMapper = vacancyDisplayMapper;
            _reviewSummaryService = reviewSummaryService;
            _providerRelationshipsService = providerRelationshipsService;
            _legalEntityAgreementService = legalEntityAgreementService;
            _trainingProviderAgreementService = trainingProviderAgreementService;
            _messaging = messaging;
            _utility = utility;
            _serviceParameters = serviceParameters;
            _feature = feature;
        }

        public async Task<VacancyPreviewViewModel> GetVacancyPreviewViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancyTask = _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Vacancy_Preview_Get);
            var programmesTask = _vacancyClient.GetActiveApprenticeshipProgrammesAsync();
           
            await Task.WhenAll(vacancyTask, programmesTask);

            var vacancy = vacancyTask.Result;
            var programme = programmesTask.Result.SingleOrDefault(p => p.Id == vacancy.ProgrammeId);
            var hasProviderReviewPermission = await _providerRelationshipsService.HasProviderGotEmployersPermissionAsync(vrm.Ukprn, vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId, OperationType.RecruitmentRequiresReview);

            var vm = new VacancyPreviewViewModel(_feature.IsFeatureEnabled(FeatureNames.FaaV2Improvements));
            await _vacancyDisplayMapper.MapFromVacancyAsync(vm, vacancy);
            
            vm.HasWage = vacancy.Wage != null;
            vm.CanShowReference = vacancy.Status != VacancyStatus.Draft;
            vm.CanShowDraftHeader = vacancy.Status == VacancyStatus.Draft;
            vm.SoftValidationErrors = GetSoftValidationErrors(vacancy);
            vm.RequiresEmployerReview = hasProviderReviewPermission;
            vm.WageType = vacancy.Wage?.WageType;

            if (programme != null) vm.ApprenticeshipLevel = programme.ApprenticeshipLevel;
            
            vm.Ukprn = vrm.Ukprn;
            vm.VacancyId = vrm.VacancyId;

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value, ReviewFieldMappingLookups.GetPreviewReviewFieldIndicators());
            }
            
            return vm;
        }

        private EntityValidationResult ValidateVacancy(Vacancy vacancy, VacancyRuleSet rules)
        {
            var result = _vacancyClient.Validate(vacancy, rules);
            FlattenErrors(result.Errors);
            return result;
        }

        public async Task<OrchestratorResponse<SubmitVacancyResponse>> SubmitVacancyAsync(SubmitEditModel m, VacancyUser user)
        {
            var vacancy = await _utility.GetAuthorisedVacancyAsync(m, RouteNames.Preview_Submit_Post);
            
            if (!vacancy.CanSubmit)
                throw new InvalidStateException(string.Format(ErrMsg.VacancyNotAvailableForEditing, vacancy.Title));
            
            vacancy.EmployerName = await _vacancyClient.GetEmployerNameAsync(vacancy);

            return await ValidateAndExecute(
                vacancy,
                v => ValidateVacancy(v, SubmitValidationRules),
                v => SubmitActionAsync(vacancy, user)
                );
        }

        private async Task<SubmitVacancyResponse> SubmitActionAsync(Vacancy vacancy, VacancyUser user)
        {
            var hasLegalEntityAgreementTask = _legalEntityAgreementService.HasLegalEntityAgreementAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId);
            var hasProviderAgreementTask = _trainingProviderAgreementService.HasAgreementAsync(vacancy.TrainingProvider.Ukprn.Value);

            await Task.WhenAll(hasLegalEntityAgreementTask, hasProviderAgreementTask);

            var hasProviderReviewPermission = await _providerRelationshipsService.HasProviderGotEmployersPermissionAsync(vacancy.TrainingProvider.Ukprn.Value, vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId, OperationType.RecruitmentRequiresReview);

            var response = new SubmitVacancyResponse
            {
                HasLegalEntityAgreement = hasLegalEntityAgreementTask.Result,
                HasProviderAgreement = hasProviderAgreementTask.Result,
                IsSubmitted = false
            };

            if (response.HasLegalEntityAgreement && response.HasProviderAgreement)
            {
                if (hasProviderReviewPermission && _serviceParameters.VacancyType.GetValueOrDefault() == VacancyType.Apprenticeship)
                {
                    var command = new ReviewVacancyCommand(vacancy.Id, user, OwnerType.Provider);
                    await _messaging.SendCommandAsync(command);
                    response.IsSentForReview = true;
                }
                else
                {
                    var command = new SubmitVacancyCommand(vacancy.Id, user, OwnerType.Provider);
                    await _messaging.SendCommandAsync(command);
                    response.IsSubmitted = true;
                }
            }
            
            return response;
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
            mappings.Add(e => e.AdditionalTrainingDescription, vm => vm.AdditionalTrainingDescription);
            mappings.Add(e => e.OutcomeDescription, vm => vm.OutcomeDescription);
            mappings.Add(e => e.Skills, vm => vm.Skills);
            mappings.Add(e => e.Qualifications, vm => vm.Qualifications);
            mappings.Add(e => e.ThingsToConsider, vm => vm.ThingsToConsider);
            mappings.Add(e => e.EmployerName, vm => vm.EmployerName);
            mappings.Add(e => e.EmployerDescription, vm => vm.EmployerDescription);
            mappings.Add(e => e.EmployerWebsiteUrl, vm => vm.EmployerWebsiteUrl);
            mappings.Add(e => e.ProviderContact.Name, vm => vm.ProviderContactName);
            mappings.Add(e => e.ProviderContact.Email, vm => vm.ProviderContactEmail);
            mappings.Add(e => e.ProviderContact.Phone, vm => vm.ProviderContactTelephone);
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
