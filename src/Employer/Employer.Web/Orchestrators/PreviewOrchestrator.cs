using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.Preview;
using Esfa.Recruit.Vacancies.Client.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Humanizer;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class PreviewOrchestrator
    {
        private readonly IVacancyClient _client;
        private readonly IGeocodeImageService _mapService;

        public PreviewOrchestrator(IVacancyClient client, IGeocodeImageService mapService)
        {
            _client = client;
            _mapService = mapService;
        }

        public async Task<PreviewVacancyViewModel> GetPreviewVacancyViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            if (vacancy.Status != VacancyStatus.Draft)
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            var vm = new PreviewVacancyViewModel
            {
                ApplicationInstructions = vacancy.ApplicationInstructions,
                ApplicationUrl = vacancy.ApplicationUrl,
                CanDelete = vacancy.CanDelete,
                CanSubmit = vacancy.CanSubmit,
                ContactName = vacancy.EmployerContactName,
                ContactEmail = vacancy.EmployerContactEmail,
                ContactTelephone = vacancy.EmployerContactPhone,
                ClosingDate = vacancy.ClosingDate.Value.ToString("dd MMM yyyy"),
                EmployerDescription = vacancy.EmployerDescription,
                EmployerName = vacancy.OrganisationName,
                EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl,
                ExpectedDuration = vacancy.Wage.DurationUnit.Value.GetDisplayName().ToQuantity(vacancy.Wage.Duration.Value),
                HoursPerWeek = vacancy.Wage.ToHoursPerWeekText(),
                Location = vacancy.Location,
                MapUrl = vacancy.Location.HasGeocode
                    ? _mapService.GetMapImageUrl(vacancy.Location.Latitude.ToString(), vacancy.Location.Longitude.ToString())
                    : _mapService.GetMapImageUrl(vacancy.Location?.Postcode),
                NumberOfPositions = vacancy.NumberOfPositions.Value,
                OutcomeDescription = vacancy.OutcomeDescription,
                PossibleStartDate = vacancy.StartDate.Value.ToString("dd MMM yyyy"),
                ProviderName = vacancy.ProviderName,
                ProviderAddress = vacancy.ProviderAddress,
                Qualifications = vacancy.Qualifications.AsText(),
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
                WageText = $"£{vacancy.Wage.FixedWageYearlyAmount?.AsMoney()}",
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
