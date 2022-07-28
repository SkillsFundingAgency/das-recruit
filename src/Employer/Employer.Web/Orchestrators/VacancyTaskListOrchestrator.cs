using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Preview;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class VacancyTaskListOrchestrator : EntityValidatingOrchestrator<Vacancy, VacancyPreviewViewModel>
    {
        private readonly IRecruitVacancyClient _recruitVacancyClient;
        private readonly IUtility _utility;
        private readonly IEmployerVacancyClient _employerVacancyClient;
        private readonly DisplayVacancyViewModelMapper _displayVacancyViewModelMapper;
        private readonly IReviewSummaryService _reviewSummaryService;
        private const VacancyRuleSet SoftValidationRules = VacancyRuleSet.MinimumWage | VacancyRuleSet.TrainingExpiryDate;

        public VacancyTaskListOrchestrator(ILogger<VacancyTaskListOrchestrator> logger,IRecruitVacancyClient recruitVacancyClient, IUtility utility, 
            IEmployerVacancyClient employerVacancyClient,  DisplayVacancyViewModelMapper displayVacancyViewModelMapper,IReviewSummaryService reviewSummaryService) : base(logger)
        {
            _recruitVacancyClient = recruitVacancyClient;
            _utility = utility;
            _employerVacancyClient = employerVacancyClient;
            _displayVacancyViewModelMapper = displayVacancyViewModelMapper;
            _reviewSummaryService = reviewSummaryService;
        }

        public async Task<VacancyPreviewViewModel> GetVacancyTaskListModel(VacancyRouteModel vrm)
        {
            var vacancyTask = _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.EmployerTaskListGet);
            var programmesTask = _recruitVacancyClient.GetActiveApprenticeshipProgrammesAsync();
            var getEmployerDataTask = _employerVacancyClient.GetEditVacancyInfoAsync(vrm.EmployerAccountId);

            await Task.WhenAll(vacancyTask, programmesTask, getEmployerDataTask);

            var vacancy = vacancyTask.Result;
            var programme = programmesTask.Result.SingleOrDefault(p => p.Id == vacancy.ProgrammeId);

            var vm = new VacancyPreviewViewModel();
            await _displayVacancyViewModelMapper.MapFromVacancyAsync(vm, vacancy);

            vm.RejectedReason = vacancy.EmployerRejectedReason;
            vm.HasProgramme = vacancy.ProgrammeId != null;
            vm.HasWage = vacancy.Wage != null;
            vm.CanShowReference = vacancy.Status != VacancyStatus.Draft;
            vm.CanShowDraftHeader = vacancy.Status == VacancyStatus.Draft;
            vm.SoftValidationErrors = GetSoftValidationErrors(vacancy);
            
            if (programme != null)
            {
                vm.ApprenticeshipLevel = programme.ApprenticeshipLevel;
            }
            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value, 
                    ReviewFieldMappingLookups.GetPreviewReviewFieldIndicators());
            }

            vm.AccountLegalEntityCount = getEmployerDataTask.Result.LegalEntities.Count();
            return vm;
        }

        public async Task<VacancyPreviewViewModel> GetCreateVacancyTaskListModel(VacancyRouteModel vrm)
        {
            var getEmployerData = await _employerVacancyClient.GetEditVacancyInfoAsync(vrm.EmployerAccountId);
            var vm = new VacancyPreviewViewModel
            {
                AccountLegalEntityCount = getEmployerData?.LegalEntities?.Count() ?? 0
            };

            return vm;
        }
        
        public async Task ClearRejectedVacancyReason(SubmitReviewModel m, VacancyUser user)
        {
            var vacancy = await _utility.GetAuthorisedVacancyAsync(m, RouteNames.ApproveJobAdvert_Post);

            vacancy.EmployerRejectedReason = null;

            await _recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, user);
        }

        public async Task UpdateRejectedVacancyReason(SubmitReviewModel m, VacancyUser user)
        {
            var vacancy = await _utility.GetAuthorisedVacancyAsync(m, RouteNames.ApproveJobAdvert_Post);

            vacancy.EmployerRejectedReason = m.RejectedReason;

            await _recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, user);
        }
        
        private EntityValidationResult GetSoftValidationErrors(Vacancy vacancy)
        {
            var result = ValidateVacancy(vacancy, SoftValidationRules);
            MapValidationPropertiesToViewModel(result);
            return result;
        }
        
        private EntityValidationResult ValidateVacancy(Vacancy vacancy, VacancyRuleSet rules)
        {
            var result = _recruitVacancyClient.Validate(vacancy, rules);
            FlattenErrors(result.Errors);
            return result;
        }
        
        private void FlattenErrors(IList<EntityValidationError> errors)
        {
            //Flatten Qualification errors to its ViewModel parent instead. 'Qualifications[1].Grade' becomes 'Qualifications'
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