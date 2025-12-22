using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.Models;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.Validation;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
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

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class VacancyCheckYourAnswersOrchestrator(
        ILogger<VacancyCheckYourAnswersOrchestrator> logger,
        DisplayVacancyViewModelMapper vacancyDisplayMapper,
        IUtility utility,
        IProviderVacancyClient providerVacancyClient,
        IRecruitVacancyClient vacancyClient,
        IReviewSummaryService reviewSummaryService,
        IProviderRelationshipsService providerRelationshipsService,
        ILegalEntityAgreementService legalEntityAgreementService,
        ITrainingProviderAgreementService trainingProviderAgreementService,
        IMessaging messaging,
        ILocationsService locationsService)
        : EntityValidatingOrchestrator<Vacancy, VacancyPreviewViewModel>(logger)
    {
        private const VacancyRuleSet SubmitValidationRules = VacancyRuleSet.All;
        private const VacancyRuleSet SoftValidationRules = VacancyRuleSet.MinimumWage |
                                                           VacancyRuleSet.TrainingExpiryDate |
                                                           VacancyRuleSet.TrainingProgramme |
                                                           VacancyRuleSet.TrainingProviderDeliverCourse |
                                                           VacancyRuleSet.EmployerName;

        public async Task<VacancyPreviewViewModel> GetVacancyTaskListModel(VacancyRouteModel routeModel)
        {
            var vacancyTask = utility.GetAuthorisedVacancyForEditAsync(routeModel, RouteNames.ProviderTaskListGet);
            
            var editVacancyInfoTask = providerVacancyClient.GetProviderEditVacancyInfoAsync(routeModel.Ukprn);
            await Task.WhenAll(vacancyTask, editVacancyInfoTask);

            var employerInfo = await providerVacancyClient.GetProviderEmployerVacancyDataAsync(routeModel.Ukprn, vacancyTask.Result.EmployerAccountId);
            
            var vacancy = vacancyTask.Result;
            var hasProviderReviewPermission = await providerRelationshipsService.HasProviderGotEmployersPermissionAsync(routeModel.Ukprn, vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId, OperationType.RecruitmentRequiresReview);
            
            var vm = new VacancyPreviewViewModel();
            await vacancyDisplayMapper.MapFromVacancyAsync(vm, vacancy);

            vm.HasWage = vacancy.Wage != null;
            vm.CanShowReference = vacancy.Status != VacancyStatus.Draft;
            vm.CanShowDraftHeader = vacancy.Status == VacancyStatus.Draft;
            vm.SoftValidationErrors = await GetSoftValidationErrors(vacancy);
            vm.Ukprn = routeModel.Ukprn;
            vm.VacancyId = routeModel.VacancyId;
            vm.RequiresEmployerReview = hasProviderReviewPermission;
            vm.AdditionalQuestionCount = vacancy.ApprenticeshipType.GetValueOrDefault() == ApprenticeshipTypes.Foundation ? 3 : 4;
            
            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference!.Value, ReviewFieldMappingLookups.GetPreviewReviewFieldIndicators());
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

            return await ValidateAndExecuteAsync(
                vacancy,
                async v => await ValidateVacancyAsync(v, SubmitValidationRules),
                async _ => await SubmitActionAsync(vacancy, user)
            );
        }
        
        private async Task<SubmitVacancyResponse> SubmitActionAsync(Vacancy vacancy, VacancyUser user)
        {
            var hasLegalEntityAgreementTask = legalEntityAgreementService.HasLegalEntityAgreementAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId);
            var hasProviderAgreementTask = trainingProviderAgreementService.HasAgreementAsync(vacancy.TrainingProvider.Ukprn!.Value);
            await Task.WhenAll(hasLegalEntityAgreementTask, hasProviderAgreementTask);

            var hasProviderReviewPermission = await providerRelationshipsService.HasProviderGotEmployersPermissionAsync(
                vacancy.TrainingProvider.Ukprn.Value,
                vacancy.EmployerAccountId,
                vacancy.AccountLegalEntityPublicHashedId,
                OperationType.RecruitmentRequiresReview);
            
            var response = new SubmitVacancyResponse
            {
                HasLegalEntityAgreement = hasLegalEntityAgreementTask.Result,
                HasProviderAgreement = hasProviderAgreementTask.Result,
                IsSubmitted = false
            };

            if (response.HasLegalEntityAgreement && response.HasProviderAgreement)
            {
                if (hasProviderReviewPermission)
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
        
        private async Task<EntityValidationResult> GetSoftValidationErrors(Vacancy vacancy)
        {
            var result = await ValidateVacancyAsync(vacancy, SoftValidationRules);
            MapValidationPropertiesToViewModel(result);
            return result;
        }
        
        private async Task<EntityValidationResult> ValidateVacancyAsync(Vacancy vacancy, VacancyRuleSet rules)
        {
            var result = vacancyClient.Validate(vacancy, rules);
            var legalEntityValidation = new LegalEntityExistsValidator(legalEntityAgreementService);
            var legalEntityValidationResult = await legalEntityValidation.ValidateAsync(vacancy);
            if (!legalEntityValidationResult.IsValid)
            {
                result.Errors.AddValidationErrors(legalEntityValidationResult.Errors);
            }
            
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
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, VacancyPreviewViewModel>
            {
                { x => x.AdditionalQuestion1, vm => vm.AdditionalQuestion1 },
                { x => x.AdditionalQuestion2, vm => vm.AdditionalQuestion2 },
                { x => x.AdditionalTrainingDescription, vm => vm.AdditionalTrainingDescription },
                { x => x.ApplicationInstructions, vm => vm.ApplicationInstructions },
                { x => x.ApplicationUrl, vm => vm.ApplicationUrl },
                { x => x.ClosingDate, vm => vm.ClosingDate },
                { x => x.Description, vm => vm.VacancyDescription },
                { x => x.EmployerDescription, vm => vm.EmployerDescription },
                { x => x.EmployerLocation, vm => vm.EmployerAddressElements },
                { x => x.EmployerLocation.AddressLine1, vm => vm.EmployerAddressElements },
                { x => x.EmployerLocation.AddressLine2, vm => vm.EmployerAddressElements },
                { x => x.EmployerLocation.AddressLine3, vm => vm.EmployerAddressElements },
                { x => x.EmployerLocation.AddressLine4, vm => vm.EmployerAddressElements },
                { x => x.EmployerLocation.Latitude, vm => vm.EmployerAddressElements },
                { x => x.EmployerLocation.Longitude, vm => vm.EmployerAddressElements },
                { x => x.EmployerLocation.Postcode, vm => vm.EmployerAddressElements },
                { x => x.EmployerName, vm => vm.EmployerName },
                { x => x.EmployerWebsiteUrl, vm => vm.EmployerWebsiteUrl },
                { x => x.NumberOfPositions, vm => vm.NumberOfPositions },
                { x => x.OutcomeDescription, vm => vm.OutcomeDescription },
                { x => x.ProgrammeId, vm => vm.HasProgramme },
                { x => x.ProviderContact.Email, vm => vm.ProviderContactEmail },
                { x => x.ProviderContact.Name, vm => vm.ProviderContactName },
                { x => x.ProviderContact.Phone, vm => vm.ProviderContactTelephone },
                { x => x.Qualifications, vm => vm.Qualifications },
                { x => x.ShortDescription, vm => vm.ShortDescription },
                { x => x.Skills, vm => vm.Skills },
                { x => x.StartDate, vm => vm.PossibleStartDate },
                { x => x.ThingsToConsider, vm => vm.ThingsToConsider },
                { x => x.TrainingDescription, vm => vm.TrainingDescription },
                { x => x.TrainingProvider, vm => vm.ProviderName },
                { x => x.TrainingProvider.Ukprn, vm => vm.ProviderName },
                { x => x.Wage, vm => vm.HasWage },
                { x => x.Wage.Duration, vm => vm.ExpectedDuration },
                { x => x.Wage.DurationUnit, vm => vm.ExpectedDuration },
                { x => x.Wage.FixedWageYearlyAmount, vm => vm.WageText },
                { x => x.Wage.WageType, vm => vm.WageText },
                { x => x.Wage.WeeklyHours, vm => vm.HoursPerWeek },
                { x => x.Wage.WorkingWeekDescription, vm => vm.WorkingWeekDescription },
            };

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