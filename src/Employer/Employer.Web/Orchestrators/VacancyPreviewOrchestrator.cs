using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.Preview;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services.MinimumWage;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Employer.Web.Extensions;
using System.Linq;
using Microsoft.Extensions.Options;
using Humanizer;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class VacancyPreviewOrchestrator
    {
        private readonly IVacancyClient _client;
        private readonly IGeocodeImageService _mapService;
        private readonly IGetMinimumWages _wageService;
        private readonly QualificationsConfiguration _qualificationsConfiguration;

        public VacancyPreviewOrchestrator(IVacancyClient client, IGeocodeImageService mapService, IGetMinimumWages wageService, IOptions<QualificationsConfiguration> qualificationsConfigOptions)
        {
            _client = client;
            _mapService = mapService;
            _wageService = wageService;
            _qualificationsConfiguration = qualificationsConfigOptions.Value;
        }

        public async Task<VacancyPreviewViewModel> GetVacancyPreviewViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            if (vacancy.Status != VacancyStatus.Draft)
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

        public Task<bool> TrySubmitVacancyAsync(SubmitEditModel m)
        {
            return _client.SubmitVacancyAsync(m.VacancyId);
        }
    }
}
