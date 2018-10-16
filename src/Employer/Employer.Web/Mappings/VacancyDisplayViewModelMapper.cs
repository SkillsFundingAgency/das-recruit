using System;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Humanizer;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;

namespace Esfa.Recruit.Employer.Web.Mappings
{
    public class DisplayVacancyViewModelMapper
    {
        private const int MapImageWidth = 465;
        private const int MapImageHeight = 256;
        private readonly IGeocodeImageService _mapService;
        private readonly IMinimumWageProvider _wageProvider;
        private readonly ExternalLinksConfiguration _externalLinksConfiguration;
        private readonly IEmployerVacancyClient _client;
        private readonly Lazy<IList<string>> _qualifications;

        public DisplayVacancyViewModelMapper(
                IGeocodeImageService mapService,
                IMinimumWageProvider wageProvider, 
                IOptions<ExternalLinksConfiguration> externalLinksOptions,
                IEmployerVacancyClient client)
        {
            _mapService = mapService;
            _wageProvider = wageProvider;
            _externalLinksConfiguration = externalLinksOptions.Value;
            _client = client;
            _qualifications = new Lazy<IList<string>>(() => _client.GetCandidateQualificationsAsync().Result.QualificationTypes);
        }

        public async Task MapFromVacancyAsync(DisplayVacancyViewModel vm, Vacancy vacancy)
        {
            var programme = await _client.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId);
            var employerProfile = await _client.GetEmployerProfileAsync(vacancy.EmployerAccountId, vacancy.LegalEntityId);

            vm.ApplicationMethod = vacancy.ApplicationMethod;
            vm.ApplicationInstructions = vacancy.ApplicationInstructions;
            vm.ApplicationUrl = vacancy.ApplicationUrl;
            vm.CanDelete = vacancy.CanDelete;
            vm.CanSubmit = vacancy.CanSubmit;
            vm.ContactName = vacancy.EmployerContactName;
            vm.ContactEmail = vacancy.EmployerContactEmail;
            vm.ContactTelephone = vacancy.EmployerContactPhone;
            vm.ClosingDate = vacancy.ClosingDate?.AsGdsDate();
            // TODO: LWA - Move this into somewhere shared that the Employer Orchestrator can use?
            vm.EmployerDescription = employerProfile?.AboutOrganisation ?? string.Empty;
            vm.EmployerName = vacancy.EmployerName;
            vm.EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl;
            vm.EmployerAddressElements = Enumerable.Empty<string>();
            vm.FindAnApprenticeshipUrl = _externalLinksConfiguration.FindAnApprenticeshipUrl;
            vm.NumberOfPositions = vacancy.NumberOfPositions?.ToString();
            vm.NumberOfPositionsCaption = vacancy.NumberOfPositions.HasValue
                ? $"{"position".ToQuantity(vacancy.NumberOfPositions.Value)} available"
                : null;
            vm.OutcomeDescription = vacancy.OutcomeDescription;
            vm.PossibleStartDate = vacancy.StartDate?.AsGdsDate();
            vm.ProviderName = vacancy.TrainingProvider?.Name;
            vm.Qualifications = vacancy.Qualifications.SortQualifications(_qualifications.Value).AsText();
            vm.ShortDescription = vacancy.ShortDescription;
            vm.Skills = vacancy.Skills ?? Enumerable.Empty<string>();
            vm.ThingsToConsider = vacancy.ThingsToConsider;
            vm.Title = vacancy.Title;
            vm.TrainingDescription = vacancy.TrainingDescription;
            vm.VacancyDescription = vacancy.Description;
            vm.VacancyReferenceNumber = vacancy.VacancyReference.HasValue 
                                        ? $"VAC{vacancy.VacancyReference.ToString()}" 
                                        : string.Empty;
            vm.IsDisabilityConfident = vacancy.IsDisabilityConfident;
            if (vacancy.EmployerLocation != null)
            {
                vm.MapUrl = vacancy.EmployerLocation.HasGeocode
                    ? _mapService.GetMapImageUrl(vacancy.EmployerLocation.Latitude.ToString(),
                        vacancy.EmployerLocation.Longitude.ToString(), MapImageWidth, MapImageHeight)
                    : _mapService.GetMapImageUrl(vacancy.EmployerLocation.Postcode, MapImageWidth, MapImageHeight);
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

            if (vacancy.ProgrammeId != null)
            {
                vm.TrainingTitle = programme?.Title;
                vm.TrainingType = programme?.ApprenticeshipType.GetDisplayName();
                vm.TrainingLevel = programme?.Level.GetDisplayName();
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
                        () => _wageProvider.GetNationalMinimumWageRange(vacancy.StartDate.Value),
                        () => _wageProvider.GetApprenticeNationalMinimumWage(vacancy.StartDate.Value))
                    : null;
                vm.WorkingWeekDescription = vacancy.Wage.WorkingWeekDescription;
            }
        }
    }
}