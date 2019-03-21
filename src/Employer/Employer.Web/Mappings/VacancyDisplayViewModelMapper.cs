using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Humanizer;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Mappings
{
    public class DisplayVacancyViewModelMapper
    {
        private const int MapImageWidth = 465;
        private const int MapImageHeight = 256;
        private readonly IGeocodeImageService _mapService;
        private readonly ExternalLinksConfiguration _externalLinksConfiguration;
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;

        public DisplayVacancyViewModelMapper(
                IGeocodeImageService mapService,
                IOptions<ExternalLinksConfiguration> externalLinksOptions,
                IEmployerVacancyClient client,
                IRecruitVacancyClient vacancyClient)
        {
            _mapService = mapService;
            _externalLinksConfiguration = externalLinksOptions.Value;
            _client = client;
            _vacancyClient = vacancyClient;
        }

        public async Task MapFromVacancyAsync(DisplayVacancyViewModel vm, Vacancy vacancy)
        {
            var programme = await _vacancyClient.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId);

            var allQualifications = await _vacancyClient.GetCandidateQualificationsAsync();

            vm.ApplicationMethod = vacancy.ApplicationMethod;
            vm.ApplicationInstructions = vacancy.ApplicationInstructions;
            vm.ApplicationUrl = vacancy.ApplicationUrl;
            vm.CanDelete = vacancy.CanDelete;
            vm.CanSubmit = vacancy.CanSubmit;
            vm.ClosingDate = vacancy.ClosingDate?.AsGdsDate();
            vm.EmployerContactName = vacancy.EmployerContact?.Name;
            vm.EmployerContactEmail = vacancy.EmployerContact?.Email;
            vm.EmployerContactTelephone = vacancy.EmployerContact?.Phone;
            vm.EmployerDescription = await _vacancyClient.GetEmployerDescriptionAsync(vacancy);
            vm.EmployerName = await _vacancyClient.GetEmployerName(vacancy.Id);
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
            vm.Qualifications = vacancy.Qualifications.SortQualifications(allQualifications).AsText();
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
                vm.WageText = vacancy.StartDate.HasValue ? vacancy.Wage.ToText(vacancy.StartDate) : null;
                vm.WorkingWeekDescription = vacancy.Wage.WorkingWeekDescription;
            }
        }
    }
}