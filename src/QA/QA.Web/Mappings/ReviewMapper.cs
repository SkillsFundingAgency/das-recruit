using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Services.MinimumWage;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Qa.Web.Mappings
{
    public class ReviewMapper
    {
        private const int MapImageWidth = 465;
        private const int MapImageHeight = 256;
        private readonly ILogger<ReviewMapper> _logger;
        private readonly IQaVacancyClient _vacancyClient;
        private readonly IGeocodeImageService _mapService;
        private readonly QualificationsConfiguration _qualificationsConfiguration;
        private readonly IGetMinimumWages _wageService;

        public ReviewMapper(ILogger<ReviewMapper> logger,
                    IQaVacancyClient vacancyClient, 
                    IGeocodeImageService mapService, 
                    IOptions<QualificationsConfiguration> qualificationsConfigOptions,
                    IGetMinimumWages wageService)
        {
            _logger = logger;
            _vacancyClient = vacancyClient;
            _mapService = mapService;
            _qualificationsConfiguration = qualificationsConfigOptions.Value;
            _wageService = wageService;
        }
        
        public async Task<ReviewViewModel> MapFromVacancy(Vacancy vacancy)
        {
            var programme = await _vacancyClient.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId);

            var vm = new ReviewViewModel();

            try
            {
                vm.SubmittedByName = vacancy.SubmittedByUser.Name;
                vm.SubmittedByEmail = vacancy.SubmittedByUser.Email;
                vm.ApplicationInstructions = vacancy.ApplicationInstructions;
                vm.ApplicationMethod = vacancy.ApplicationMethod.Value;
                vm.ApplicationUrl = vacancy.ApplicationUrl;
                vm.ContactName = vacancy.EmployerContactName;
                vm.ContactEmail = vacancy.EmployerContactEmail;
                vm.ContactTelephone = vacancy.EmployerContactPhone;
                vm.ClosingDate = vacancy.ClosingDate?.AsDisplayDate();
                vm.EmployerDescription = vacancy.EmployerDescription;
                vm.EmployerName = vacancy.EmployerName;
                vm.EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl;
                SetEmployerAddressElements(vm, vacancy);
                vm.NumberOfPositionsCaption =  $"{"position".ToQuantity(vacancy.NumberOfPositions.Value)} available";
                vm.OutcomeDescription = vacancy.OutcomeDescription;
                vm.PossibleStartDate = vacancy.StartDate?.AsDisplayDate();
                vm.ProviderName = vacancy.TrainingProvider.Name;
                vm.Qualifications = vacancy.Qualifications.SortQualifications(_qualificationsConfiguration.QualificationTypes).AsText();
                vm.ShortDescription = vacancy.ShortDescription;
                vm.Skills = vacancy.Skills ?? Enumerable.Empty<string>();
                vm.ThingsToConsider = vacancy.ThingsToConsider;
                vm.Title = vacancy.Title;
                vm.TrainingDescription = vacancy.TrainingDescription;
                vm.VacancyDescription = vacancy.Description;
                vm.VacancyReferenceNumber = $"VAC{vacancy.VacancyReference.ToString()}";
                vm.TrainingTitle = programme.Title;
                vm.TrainingType = programme.ApprenticeshipType.GetDisplayName();
                vm.TrainingLevel = programme.Level.GetDisplayName();
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
            catch (NullReferenceException ex)
            {
                _logger.LogError(ex, "Unable to map vacancy to view model. Unexpected null fields.");
                throw;
            }
            
            return vm;
        }
        
        private void SetEmployerAddressElements(ReviewViewModel vm, Vacancy vacancy)
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
    }
}