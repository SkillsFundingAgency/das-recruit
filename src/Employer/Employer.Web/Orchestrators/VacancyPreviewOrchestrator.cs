using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.Preview;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services.MinimumWage;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Rest;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class VacancyPreviewOrchestrator : EntityValidatingOrchestrator<Vacancy, VacancyPreviewViewModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.All;
        private readonly IVacancyClient _client;
        private readonly IGeocodeImageService _mapService;
        private readonly IGetMinimumWages _wageService;
        private readonly QualificationsConfiguration _qualificationsConfiguration;
        
        public VacancyPreviewOrchestrator(IVacancyClient client, IGeocodeImageService mapService, IGetMinimumWages wageService, 
            IOptions<QualificationsConfiguration> qualificationsConfigOptions, ILogger<VacancyPreviewOrchestrator> logger) : base(logger)
        {
            _client = client;
            _mapService = mapService;
            _wageService = wageService;
            _qualificationsConfiguration = qualificationsConfigOptions.Value;
        }

        public async Task<VacancyPreviewViewModel> GetVacancyPreviewViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            if (!vacancy.CanEdit)
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            
            var vm = new VacancyPreviewViewModel
            {
                ApplicationInstructions = vacancy.ApplicationInstructions,
                ApplicationUrl = vacancy.ApplicationUrl,
                CanDelete = vacancy.CanDelete,
                CanSubmit = vacancy.CanSubmit,
                ContactName = vacancy.EmployerContactName,
                ContactEmail = vacancy.EmployerContactEmail,
                ContactTelephone = vacancy.EmployerContactPhone,
                ClosingDate = vacancy.ClosingDate.Value.AsDisplayDate(),
                EmployerDescription = vacancy.EmployerDescription,
                EmployerName = vacancy.EmployerName,
                EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl,
                ExpectedDuration = vacancy.Wage.DurationUnit.Value.GetDisplayName().ToQuantity(vacancy.Wage.Duration.Value),
                HoursPerWeek = $"{vacancy.Wage.WeeklyHours:0.##}",
                Location = vacancy.EmployerLocation,
                MapUrl = vacancy.EmployerLocation.HasGeocode
                    ? _mapService.GetMapImageUrl(vacancy.EmployerLocation.Latitude.ToString(), vacancy.EmployerLocation.Longitude.ToString())
                    : _mapService.GetMapImageUrl(vacancy.EmployerLocation?.Postcode),
                NumberOfPositions = vacancy.NumberOfPositions.Value,
                OutcomeDescription = vacancy.OutcomeDescription,
                PossibleStartDate = vacancy.StartDate.Value.AsDisplayDate(),
                ProviderName = vacancy.TrainingProvider?.Name,
                ProviderAddress = vacancy.TrainingProvider?.Address?.GetInlineAddress(),
                Qualifications = vacancy.Qualifications.SortQualifications(_qualificationsConfiguration.QualificationTypes).AsText(),
                ShortDescription = vacancy.ShortDescription,
                Skills = vacancy.Skills ?? Enumerable.Empty<string>(),
                ThingsToConsider = vacancy.ThingsToConsider,
                Title = vacancy.Title,
                TrainingDescription = vacancy.TrainingDescription,
                TrainingTitle = vacancy.Programme.Title,
                TrainingType = vacancy.Programme.TrainingType?.GetDisplayName(),
                TrainingLevel = vacancy.Programme.LevelName,
                VacancyDescription = vacancy.Description,
                VacancyReferenceNumber = string.Empty,
                WageInfo = vacancy.Wage.WageAdditionalInformation,
                WageText = vacancy.Wage?.ToText(
                    () => _wageService.GetNationalMinimumWageRange(vacancy.StartDate.Value),
                    () => _wageService.GetApprenticeNationalMinimumWage(vacancy.StartDate.Value)),
                WorkingWeekDescription = vacancy.Wage.WorkingWeekDescription
            };
            
            return vm;
        }
        
        public async Task<OrchestratorResponse<bool>> TrySubmitVacancyAsync(SubmitEditModel m)
        {
            var vacancy = await _client.GetVacancyForEditAsync(m.VacancyId);

            if (!vacancy.CanEdit)
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            return await ValidateAndExecute(
                vacancy,
                v => _client.Validate(v, ValidationRules),
                v => Task.FromResult(false) //_client.SubmitVacancyAsync(v.Id)
            );
        }
        
        protected override EntityToViewModelPropertyMappings<Vacancy, VacancyPreviewViewModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, VacancyPreviewViewModel>();

            mappings.Add(e => e.Description, vm => vm.VacancyDescription);
            mappings.Add(e => e.TrainingDescription, vm => vm.TrainingDescription);
            mappings.Add(e => e.OutcomeDescription, vm => vm.OutcomeDescription);
            mappings.Add(e => e.Skills, vm => vm.Skills);
            mappings.Add(e => e.Qualifications, vm => vm.Qualifications);

            return mappings;
        }
    }
}
