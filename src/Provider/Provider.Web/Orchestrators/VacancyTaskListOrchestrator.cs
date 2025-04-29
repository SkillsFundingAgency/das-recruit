using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Locations;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using Microsoft.Extensions.Logging;
using ErrorMessages = Esfa.Recruit.Shared.Web.ViewModels.ErrorMessages;
using FeatureNames = Esfa.Recruit.Provider.Web.Configuration.FeatureNames;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class VacancyTaskListOrchestrator(
        ILogger<VacancyTaskListOrchestrator> logger,
        DisplayVacancyViewModelMapper vacancyDisplayMapper,
        IUtility utility,
        IProviderVacancyClient providerVacancyClient,
        IRecruitVacancyClient vacancyClient,
        IReviewSummaryService reviewSummaryService,
        IProviderRelationshipsService providerRelationshipsService,
        ILegalEntityAgreementService legalEntityAgreementService,
        ITrainingProviderAgreementService trainingProviderAgreementService,
        IMessaging messaging,
        ServiceParameters serviceParameters,
        ILocationsService locationsService,
        IFeature feature)
        : EntityValidatingOrchestrator<Vacancy, VacancyPreviewViewModel>(logger)
    {
        private const VacancyRuleSet SubmitValidationRules = VacancyRuleSet.All;
        private const VacancyRuleSet SoftValidationRules = VacancyRuleSet.MinimumWage | VacancyRuleSet.TrainingExpiryDate;

        public async Task<VacancyPreviewViewModel> GetVacancyTaskListModel(VacancyRouteModel routeModel)
        {
            var vacancyTask = utility.GetAuthorisedVacancyForEditAsync(routeModel, RouteNames.ProviderTaskListGet);
            
            var editVacancyInfoTask = providerVacancyClient.GetProviderEditVacancyInfoAsync(routeModel.Ukprn);
            await Task.WhenAll(vacancyTask, editVacancyInfoTask);

            var employerInfo = await providerVacancyClient.GetProviderEmployerVacancyDataAsync(routeModel.Ukprn, vacancyTask.Result.EmployerAccountId);
            
            var vacancy = vacancyTask.Result;
            var hasProviderReviewPermission = await providerRelationshipsService.HasProviderGotEmployersPermissionAsync(routeModel.Ukprn, vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId, OperationType.RecruitmentRequiresReview);
            
            var vm = new VacancyPreviewViewModel(feature.IsFeatureEnabled(FeatureNames.MultipleLocations));
            await vacancyDisplayMapper.MapFromVacancyAsync(vm, vacancy);

            vm.HasWage = vacancy.Wage != null;
            vm.CanShowReference = vacancy.Status != VacancyStatus.Draft;
            vm.CanShowDraftHeader = vacancy.Status == VacancyStatus.Draft;
            vm.SoftValidationErrors = GetSoftValidationErrors(vacancy);
            vm.Ukprn = routeModel.Ukprn;
            vm.VacancyId = routeModel.VacancyId;
            vm.RequiresEmployerReview = hasProviderReviewPermission;
            
            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value, ReviewFieldMappingLookups.GetPreviewReviewFieldIndicators());
            }

            vm.AccountLegalEntityCount = employerInfo?.LegalEntities.Count ?? 0;
            vm.AccountCount = editVacancyInfoTask.Result.Employers.Count();
            return vm;
        }

        public async Task<VacancyPreviewViewModel> GetCreateVacancyTaskListModel(VacancyRouteModel vrm, string employerAccountId)
        {
            var employerInfo =
                await providerVacancyClient.GetProviderEmployerVacancyDataAsync(vrm.Ukprn,
                    employerAccountId);
            var editVacancyInfo = await providerVacancyClient.GetProviderEditVacancyInfoAsync(vrm.Ukprn);

            var createVacancyTaskListModel = new VacancyPreviewViewModel
            {
                AccountCount = editVacancyInfo.Employers.Count(),
                AccountLegalEntityCount = employerInfo.LegalEntities.Count,
                AccountId = employerAccountId,
                Ukprn = vrm.Ukprn,
                VacancyId = null
            };
            return createVacancyTaskListModel;
        }
        
        public async Task<OrchestratorResponse<SubmitVacancyResponse>> SubmitVacancyAsync(SubmitEditModel m, VacancyUser user)
        {
            var vacancy = await utility.GetAuthorisedVacancyAsync(m, RouteNames.Preview_Submit_Post);
            
            if (!vacancy.CanSubmit)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            
            vacancy.EmployerName = await vacancyClient.GetEmployerNameAsync(vacancy);
            
            await UpdateAddressCountriesAsync(vacancy, user);

            return await ValidateAndExecute(
                vacancy,
                v => ValidateVacancy(v, SubmitValidationRules),
                v => SubmitActionAsync(vacancy, user)
            );
        }
        
        private async Task<SubmitVacancyResponse> SubmitActionAsync(Vacancy vacancy, VacancyUser user)
        {
            var hasLegalEntityAgreementTask = legalEntityAgreementService.HasLegalEntityAgreementAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId);
            var hasProviderAgreementTask = trainingProviderAgreementService.HasAgreementAsync(vacancy.TrainingProvider.Ukprn.Value);

            await Task.WhenAll(hasLegalEntityAgreementTask, hasProviderAgreementTask);

            var hasProviderReviewPermission = await providerRelationshipsService.HasProviderGotEmployersPermissionAsync(vacancy.TrainingProvider.Ukprn.Value, vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId, OperationType.RecruitmentRequiresReview);

            var response = new SubmitVacancyResponse
            {
                HasLegalEntityAgreement = hasLegalEntityAgreementTask.Result,
                HasProviderAgreement = hasProviderAgreementTask.Result,
                IsSubmitted = false
            };

            if (response.HasLegalEntityAgreement && response.HasProviderAgreement)
            {
                if (hasProviderReviewPermission && serviceParameters.VacancyType.GetValueOrDefault() == VacancyType.Apprenticeship)
                {
                    var command = new ReviewVacancyCommand(vacancy.Id, user, OwnerType.Provider);
                    await messaging.SendCommandAsync(command);
                    response.IsSentForReview = true;
                }
                else
                {
                    var command = new SubmitVacancyCommand(vacancy.Id, user, OwnerType.Provider);
                    await messaging.SendCommandAsync(command);
                    response.IsSubmitted = true;
                }
            }
            
            return response;
        }
        
        private EntityValidationResult GetSoftValidationErrors(Vacancy vacancy)
        {
            var result = ValidateVacancy(vacancy, SoftValidationRules);
            MapValidationPropertiesToViewModel(result);
            return result;
        }
        
        private EntityValidationResult ValidateVacancy(Vacancy vacancy, VacancyRuleSet rules)
        {
            var result = vacancyClient.Validate(vacancy, rules);
            result.Errors = FlattenErrors(result.Errors, vacancy);
            return result;
        }
        
        private static List<EntityValidationError> FlattenErrors(IList<EntityValidationError> errors, Vacancy vacancy)
        {
            //Flatten Qualification errors to its ViewModel parent instead. 'Qualifications[1].Grade' becomes 'Qualifications'
            foreach (var error in errors.Where(e => e.PropertyName.StartsWith(nameof(Vacancy.Qualifications))))
            {
                error.PropertyName = nameof(VacancyPreviewViewModel.Qualifications);
            }

            //Flatten EmployerLocations errors, e.g. 'EmployerLocations[0].Country' becomes 'EmployerLocations.Country'
            var addressErrors = errors.Where(e => e.PropertyName.StartsWith("EmployerLocations[")).ToList();
            foreach (var error in addressErrors)
            {
                string[] tokens = error.PropertyName.Split('.');
                error.PropertyName = $"EmployerLocations.{tokens.LastOrDefault()}";
            }

            if (vacancy.EmployerLocationOption == AvailableWhere.MultipleLocations)
            {
                errors.Where(x => x.PropertyName == "EmployerLocations.Country").ToList().ForEach(x =>
                {
                    x.ErrorCode = $"Multiple-{VacancyValidationErrorCodes.AddressCountryNotInEngland}";
                });
            }

            return errors.DistinctBy(x => $"{x.PropertyName}: {x.ErrorMessage}").ToList();
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
            mappings.Add(e => e.AdditionalQuestion1, vm => vm.AdditionalQuestion1);
            mappings.Add(e => e.AdditionalQuestion2, vm => vm.AdditionalQuestion2);

            return mappings;
        }
        
        private async Task UpdateAddressCountriesAsync(Vacancy vacancy, VacancyUser user)
        {
            var locations = new List<Address>();
            switch (vacancy.EmployerLocationOption)
            {
                case AvailableWhere.AcrossEngland:
                    return;
                case AvailableWhere.OneLocation:
                case AvailableWhere.MultipleLocations:
                    locations.AddRange(vacancy.EmployerLocations);
                    break;
                default:
                    locations.Add(vacancy.EmployerLocation);
                    break;
            }
        
            var addressesToQuery = locations
                .Where(x => x.Country is null)
                .Select(x => x.Postcode)
                .Distinct()
                .ToList();
            var results = await locationsService.GetBulkPostcodeDataAsync(addressesToQuery);
        
            bool isDirty = false;
            locations.ForEach(x =>
            {
                if (x.Country is null && results.TryGetValue(x.Postcode, out var postcodeData) && postcodeData is not null)
                {
                    x.Country = postcodeData.Country;
                    isDirty = true;
                }
            });
        
            if (isDirty)
            {
                await vacancyClient.UpdateDraftVacancyAsync(vacancy, user);
            }
        }
    }
}