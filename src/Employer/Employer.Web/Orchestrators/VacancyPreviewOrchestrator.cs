﻿using Esfa.Recruit.Employer.Web.Extensions;
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
                ClosingDate = vacancy.ClosingDate?.AsDisplayDate(),
                EmployerDescription = vacancy.EmployerDescription,
                EmployerName = vacancy.EmployerName,
                EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl,
                EmployerAddressElements = Enumerable.Empty<string>(),
                NumberOfPositions = vacancy.NumberOfPositions?.ToString(),
                NumberOfPositionsCaption = vacancy.NumberOfPositions.HasValue ? $"{"position".ToQuantity(vacancy.NumberOfPositions.Value)} available" : null,
                OutcomeDescription = vacancy.OutcomeDescription,
                PossibleStartDate = vacancy.StartDate?.AsDisplayDate(),
                ProviderName = vacancy.TrainingProvider?.Name,
                Qualifications = vacancy.Qualifications.SortQualifications(_qualificationsConfiguration.QualificationTypes).AsText(),
                ShortDescription = vacancy.ShortDescription,
                Skills = vacancy.Skills ?? Enumerable.Empty<string>(),
                ThingsToConsider = vacancy.ThingsToConsider,
                Title = vacancy.Title,
                TrainingDescription = vacancy.TrainingDescription,
                VacancyDescription = vacancy.Description,
                VacancyReferenceNumber = string.Empty
            };

            if (vacancy.EmployerLocation != null)
            {
                vm.MapUrl = vacancy.EmployerLocation.HasGeocode
                    ? _mapService.GetMapImageUrl(vacancy.EmployerLocation.Latitude.ToString(),
                        vacancy.EmployerLocation.Longitude.ToString())
                    : _mapService.GetMapImageUrl(vacancy.EmployerLocation.Postcode);
                vm.EmployerAddressElements = new[]
                    {
                        vacancy.EmployerLocation.AddressLine1,
                        vacancy.EmployerLocation.AddressLine2,
                        vacancy.EmployerLocation.AddressLine3,
                        vacancy.EmployerLocation.AddressLine4,
                        vacancy.EmployerLocation.Postcode
                    }
                    .Where(x => !string.IsNullOrEmpty(x));
            }

            if (vacancy.Programme != null)
            {
                vm.TrainingTitle = vacancy.Programme.Title;
                vm.TrainingType = vacancy.Programme.TrainingType?.GetDisplayName();
                vm.TrainingLevel = vacancy.Programme.LevelName;
            }

            if (vacancy.Wage != null)
            {
                vm.ExpectedDuration = (vacancy.Wage.DurationUnit.HasValue && vacancy.Wage.Duration.HasValue)
                    ? vacancy.Wage.DurationUnit.Value.GetDisplayName().ToQuantity(vacancy.Wage.Duration.Value)
                    : null;
                vm.HoursPerWeek = $"{vacancy.Wage.WeeklyHours:0.##}";
                vm.WageInfo = vacancy.Wage.WageAdditionalInformation;
                vm.WageText = vacancy.StartDate.HasValue
                    ? vacancy.Wage.ToText(
                        () => _wageService.GetNationalMinimumWageRange(vacancy.StartDate.Value),
                        () => _wageService.GetApprenticeNationalMinimumWage(vacancy.StartDate.Value))
                    : null;
                vm.WorkingWeekDescription = vacancy.Wage.WorkingWeekDescription;
            }
            
            return vm;
        }
        
        public async Task<OrchestratorResponse<bool>> TrySubmitVacancyAsync(SubmitEditModel m, string user, string userEmail)
        {
            var vacancy = await _client.GetVacancyForEditAsync(m.VacancyId);

            if (!vacancy.CanEdit)
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            return await ValidateAndExecute(
                vacancy,
                v => _client.Validate(v, ValidationRules),
                v => _client.SubmitVacancyAsync(v.Id, user, userEmail)
            );
        }
        
        protected override EntityToViewModelPropertyMappings<Vacancy, VacancyPreviewViewModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, VacancyPreviewViewModel>();

            mappings.Add(e => e.ShortDescription, vm => vm.ShortDescription);
            mappings.Add(e => e.ClosingDate, vm => vm.ClosingDate);
            mappings.Add(e => e.Wage, vm => vm.Wage);
            mappings.Add(e => e.Wage.WeeklyHours, vm => vm.HoursPerWeek);
            mappings.Add(e => e.Wage.WorkingWeekDescription, vm => vm.WorkingWeekDescription);
            mappings.Add(e => e.StartDate, vm => vm.PossibleStartDate);
            mappings.Add(e => e.Programme, vm => vm.Programme);
            mappings.Add(e => e.Programme.Level, vm => vm.TrainingLevel);
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
            mappings.Add(e => e.Programme.TrainingType, vm => vm.TrainingType);
            mappings.Add(e => e.Programme.Title, vm => vm.TrainingTitle);

            return mappings;
        }
    }
}
