﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.Models;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using ErrMsg = Esfa.Recruit.Shared.Web.ViewModels.ErrorMessages;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class VacancyPreviewOrchestrator : EntityValidatingOrchestrator<Vacancy, VacancyPreviewViewModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.All;
        private readonly IProviderVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly DisplayVacancyViewModelMapper _vacancyDisplayMapper;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly ILegalEntityAgreementService _legalEntityAgreementService;
        private readonly ITrainingProviderAgreementService _trainingProviderAgreementService;

        public VacancyPreviewOrchestrator(
            IProviderVacancyClient client,
            IRecruitVacancyClient vacancyClient,
            ILogger<VacancyPreviewOrchestrator> logger,
            DisplayVacancyViewModelMapper vacancyDisplayMapper, 
            IReviewSummaryService reviewSummaryService,
            ILegalEntityAgreementService legalEntityAgreementService,
            ITrainingProviderAgreementService trainingProviderAgreementService) : base(logger)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _vacancyDisplayMapper = vacancyDisplayMapper;
            _reviewSummaryService = reviewSummaryService;
            _legalEntityAgreementService = legalEntityAgreementService;
            _trainingProviderAgreementService = trainingProviderAgreementService;
        }

        public async Task<VacancyPreviewViewModel> GetVacancyPreviewViewModelAsync(VacancyRouteModel vrm)
        {
            //var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.Vacancy_Preview_Get);
            var vacancyTask = Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.Vacancy_Preview_Get);
            var programmesTask = _vacancyClient.GetActiveApprenticeshipProgrammesAsync();

            await Task.WhenAll(vacancyTask, programmesTask);

            var programme = programmesTask.Result.SingleOrDefault(p => p.Id == vacancyTask.Result.ProgrammeId);
            var vm = new VacancyPreviewViewModel();
            await _vacancyDisplayMapper.MapFromVacancyAsync(vm, vacancyTask.Result);
            
            vm.HasProgramme = vacancyTask.Result.ProgrammeId != null;
            vm.HasWage = vacancyTask.Result.Wage != null;
            vm.CanShowReference = vacancyTask.Result.Status != VacancyStatus.Draft;
            vm.CanShowDraftHeader = vacancyTask.Result.Status == VacancyStatus.Draft;
            if (programme != null) vm.Level = programme.Level;
            if (vacancyTask.Result.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancyTask.Result.VacancyReference.Value, 
                    ReviewFieldMappingLookups.GetPreviewReviewFieldIndicators());
            }
            
            return vm;
        }
        
        public async Task<OrchestratorResponse<SubmitVacancyResponse>> SubmitVacancyAsync(SubmitEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyAsync(_client, _vacancyClient, m, RouteNames.Preview_Submit_Post);
            
            if (!vacancy.CanSubmit)
                throw new InvalidStateException(string.Format(ErrMsg.VacancyNotAvailableForEditing, vacancy.Title));
            
            vacancy.EmployerName = await _vacancyClient.GetEmployerNameAsync(vacancy);
            
            return await ValidateAndExecute(
                vacancy,
                v =>
                {
                    var result = _vacancyClient.Validate(v, ValidationRules);
                    SyncErrorsAndModel(result.Errors);
                    return result;
                },
                v => SubmitActionAsync(v, user)
                );
        }

        private async Task<SubmitVacancyResponse> SubmitActionAsync(Vacancy vacancy, VacancyUser user)
        {
            var hasLegalEntityAgreementTask = _legalEntityAgreementService.HasLegalEntityAgreementAsync(vacancy.EmployerAccountId, vacancy.LegalEntityId);
            var hasProviderAgreementTask = _trainingProviderAgreementService.HasAgreementAsync(vacancy.TrainingProvider.Ukprn.Value);

            await Task.WhenAll(hasLegalEntityAgreementTask, hasProviderAgreementTask);

            var response = new SubmitVacancyResponse
            {
                HasLegalEntityAgreement = hasLegalEntityAgreementTask.Result,
                HasProviderAgreement = hasProviderAgreementTask.Result,
                IsSubmitted = false
            };

            if (response.HasLegalEntityAgreement && response.HasProviderAgreement)
            {
                await _client.SubmitVacancyAsync(vacancy.Id, user);
                response.IsSubmitted = true;
            }
            
            return response;
        }

        private void SyncErrorsAndModel(IList<EntityValidationError> errors)
        {
            //Flatten Qualification errors to its ViewModel parent instead. 'Qualifications[1].Grade' > 'Qualifications'
            foreach (var error in errors.Where(e => e.PropertyName.StartsWith(nameof(Vacancy.Qualifications))))
            {
                error.PropertyName = nameof(VacancyPreviewViewModel.Qualifications);
            }
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, VacancyPreviewViewModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, VacancyPreviewViewModel>();

            mappings.Add(e => e.ShortDescription, vm => vm.ShortDescription);
            mappings.Add(e => e.ClosingDate, vm => vm.ClosingDate);
            mappings.Add(e => e.Wage, vm => vm.HasWage);
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
