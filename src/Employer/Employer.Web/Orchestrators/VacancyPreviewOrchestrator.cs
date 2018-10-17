using Esfa.Recruit.Employer.Web.ViewModels.Preview;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Services;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class VacancyPreviewOrchestrator : EntityValidatingOrchestrator<Vacancy, VacancyPreviewViewModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.All;
        private readonly IEmployerVacancyClient _client;
        private readonly DisplayVacancyViewModelMapper _vacancyDisplayMapper;
        private readonly IReviewSummaryService _reviewSummaryService;

        public VacancyPreviewOrchestrator(IEmployerVacancyClient client, ILogger<VacancyPreviewOrchestrator> logger,
            DisplayVacancyViewModelMapper vacancyDisplayMapper, IReviewSummaryService reviewSummaryService) : base(logger)
        {
            _client = client;
            _vacancyDisplayMapper = vacancyDisplayMapper;
            _reviewSummaryService = reviewSummaryService;
        }

        public async Task<VacancyPreviewViewModel> GetVacancyPreviewViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, vrm, RouteNames.Vacancy_Preview_Get);
            
            var vm = new VacancyPreviewViewModel();
            await _vacancyDisplayMapper.MapFromVacancyAsync(vm, vacancy);
            
            vm.HasProgramme = vacancy.ProgrammeId != null;
            vm.HasWage = vacancy.Wage != null;
            vm.CanShowReference = vacancy.Status != VacancyStatus.Draft;
            vm.CanShowDraftHeader = vacancy.Status == VacancyStatus.Draft;

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModel(vacancy.VacancyReference.Value, 
                    ReviewFieldMappingLookups.GetPreviewReviewFieldIndicators());
            }
            
            return vm;
        }
        
        public async Task<OrchestratorResponse> SubmitVacancyAsync(SubmitEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyAsync(_client, m, RouteNames.Preview_Submit_Post);

            // TODO: LWA - Move this to somewhere shared
            var employerProfile = await _client.GetEmployerProfileAsync(vacancy.EmployerAccountId, vacancy.LegalEntityId);
            vacancy.EmployerDescription = employerProfile?.AboutOrganisation ?? string.Empty;

            if (!vacancy.CanSubmit)
                throw new InvalidStateException(string.Format(ViewModels.ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            
            return await ValidateAndExecute(
                vacancy,
                v =>
                {
                    var result = _client.Validate(v, ValidationRules);
                    SyncErrorsAndModel(result.Errors);
                    return result;
                },
                v => _client.SubmitVacancyAsync(v, user)
            );
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
            mappings.Add(e => e.EmployerContactName, vm => vm.ContactName);
            mappings.Add(e => e.EmployerContactEmail, vm => vm.ContactEmail);
            mappings.Add(e => e.EmployerContactPhone, vm => vm.ContactTelephone);
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
