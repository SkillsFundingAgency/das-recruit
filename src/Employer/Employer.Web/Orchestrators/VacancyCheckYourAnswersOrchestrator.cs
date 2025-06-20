using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Preview;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Locations;
using Microsoft.Extensions.Logging;
using ErrMsg = Esfa.Recruit.Shared.Web.ViewModels.ErrorMessages;
using FeatureNames = Esfa.Recruit.Employer.Web.Configuration.FeatureNames;

namespace Esfa.Recruit.Employer.Web.Orchestrators;

public class VacancyCheckYourAnswersOrchestrator(
    ILogger<VacancyCheckYourAnswersOrchestrator> logger,
    IRecruitVacancyClient recruitVacancyClient,
    IUtility utility,
    IEmployerVacancyClient employerVacancyClient,
    DisplayVacancyViewModelMapper displayVacancyViewModelMapper,
    IReviewSummaryService reviewSummaryService,
    ILegalEntityAgreementService legalEntityAgreementService,
    IMessaging messaging,
    ILocationsService locationsService,
    IFeature feature) : EntityValidatingOrchestrator<Vacancy, VacancyPreviewViewModel>(logger)
{
    private const VacancyRuleSet SoftValidationRules = VacancyRuleSet.MinimumWage | VacancyRuleSet.TrainingExpiryDate;
    private const VacancyRuleSet SubmitValidationRules = VacancyRuleSet.All;

    public async Task<VacancyPreviewViewModel> GetVacancyTaskListModel(VacancyRouteModel vrm)
    {
        var vacancyTask = utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.EmployerTaskListGet);
        var getEmployerDataTask = employerVacancyClient.GetEditVacancyInfoAsync(vrm.EmployerAccountId);

        await Task.WhenAll(vacancyTask, getEmployerDataTask);

        var vacancy = vacancyTask.Result;
            
        var vm = new VacancyPreviewViewModel();
        await displayVacancyViewModelMapper.MapFromVacancyAsync(vm, vacancy);

        vm.RejectedReason = vacancy.EmployerRejectedReason;
        vm.HasProgramme = vacancy.ProgrammeId != null;
        vm.HasWage = vacancy.Wage != null;
        vm.CanShowReference = vacancy.Status != VacancyStatus.Draft;
        vm.CanShowDraftHeader = vacancy.Status == VacancyStatus.Draft;
        vm.SoftValidationErrors = GetSoftValidationErrors(vacancy);
        vm.VacancyId = vrm.VacancyId;
        vm.EmployerAccountId = vrm.EmployerAccountId;
        vm.AdditionalQuestionCount = vacancy.ApprenticeshipType.GetValueOrDefault() == ApprenticeshipTypes.Foundation ? 3 : 4;
            
        if (vacancy.Status == VacancyStatus.Referred)
        {
            vm.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value, 
                ReviewFieldMappingLookups.GetPreviewReviewFieldIndicators());
        }

        vm.AccountLegalEntityCount = getEmployerDataTask.Result.LegalEntities.Count();
        return vm;
    }
        
    public async Task ClearRejectedVacancyReason(SubmitReviewModel m, VacancyUser user)
    {
        var vacancy = await utility.GetAuthorisedVacancyAsync(m, RouteNames.ApproveJobAdvert_Post);

        vacancy.EmployerRejectedReason = null;

        await recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, user);
    }

    public async Task UpdateRejectedVacancyReason(SubmitReviewModel m, VacancyUser user)
    {
        var vacancy = await utility.GetAuthorisedVacancyAsync(m, RouteNames.ApproveJobAdvert_Post);

        vacancy.EmployerRejectedReason = m.RejectedReason;

        await recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, user);
    }
        
    private EntityValidationResult GetSoftValidationErrors(Vacancy vacancy)
    {
        var result = ValidateVacancy(vacancy, SoftValidationRules);
        MapValidationPropertiesToViewModel(result);
        return result;
    }
        
    private EntityValidationResult ValidateVacancy(Vacancy vacancy, VacancyRuleSet rules)
    {
        var result = recruitVacancyClient.Validate(vacancy, rules);
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

    private async Task<SubmitVacancyResponse> SubmitActionAsync(Vacancy vacancy, VacancyUser user)
    {
        var response = new SubmitVacancyResponse
        {
            HasLegalEntityAgreement = await legalEntityAgreementService.HasLegalEntityAgreementAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId),
            IsSubmitted = false
        };

        if (response.HasLegalEntityAgreement == false)
            return response;

        var command = new SubmitVacancyCommand(vacancy.Id, user,OwnerType.Employer, vacancy.EmployerDescription);

        await messaging.SendCommandAsync(command);

        response.IsSubmitted = true;

        return response;
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
        mappings.Add(e => e.EmployerContact.Name, vm => vm.EmployerContactName);
        mappings.Add(e => e.EmployerContact.Email, vm => vm.EmployerContactEmail);
        mappings.Add(e => e.EmployerContact.Phone, vm => vm.EmployerContactTelephone);
            
        if (feature.IsFeatureEnabled(FeatureNames.MultipleLocations))
        {
            mappings.Add(e => e.EmployerLocations, vm => vm.AvailableLocations);
        }
            
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

    public async Task<OrchestratorResponse<SubmitVacancyResponse>> SubmitVacancyAsync(SubmitEditModel m, VacancyUser user)
    {
        var vacancy = await utility.GetAuthorisedVacancyAsync(m, RouteNames.Preview_Submit_Post);
            
        if (!vacancy.CanSubmit)
            throw new InvalidStateException(string.Format(ErrMsg.VacancyNotAvailableForEditing, vacancy.Title));

        await UpdateAddressCountriesAsync(vacancy, user);
            
        var employerDescriptionTask = recruitVacancyClient.GetEmployerDescriptionAsync(vacancy);
        var employerNameTask = recruitVacancyClient.GetEmployerNameAsync(vacancy);
            
        await Task.WhenAll(employerDescriptionTask, employerNameTask);

        vacancy.EmployerDescription = employerDescriptionTask.Result;
        vacancy.EmployerName = employerNameTask.Result;

        return await ValidateAndExecute(
            vacancy,
            v => ValidateVacancy(v, SubmitValidationRules),
            v => SubmitActionAsync(v, user)
        );
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
            await recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, user);
        }
    }
}