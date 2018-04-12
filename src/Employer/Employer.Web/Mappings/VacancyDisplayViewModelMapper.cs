using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Services.MinimumWage;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Humanizer;
using Microsoft.Extensions.Options;
using System.Linq;

namespace Esfa.Recruit.Employer.Web.Mappings
{
    public class DisplayVacancyViewModelMapper
    {
        private readonly IGeocodeImageService _mapService;
        private readonly IGetMinimumWages _wageService;
        private readonly QualificationsConfiguration _qualificationsConfiguration;

        public DisplayVacancyViewModelMapper(IGeocodeImageService mapService, IGetMinimumWages wageService, IOptions<QualificationsConfiguration> qualificationsConfigOptions)
        {
            _mapService = mapService;
            _wageService = wageService;
            _qualificationsConfiguration = qualificationsConfigOptions.Value;
        }

        public void MapFromVacancy(DisplayVacancyViewModel vm, Vacancy vacancy)
        {
            vm.ApplicationInstructions = vacancy.ApplicationInstructions;
            vm.ApplicationUrl = vacancy.ApplicationUrl;
            vm.CanDelete = vacancy.CanDelete;
            vm.CanSubmit = vacancy.CanSubmit;
            vm.ContactName = vacancy.EmployerContactName;
            vm.ContactEmail = vacancy.EmployerContactEmail;
            vm.ContactTelephone = vacancy.EmployerContactPhone;
            vm.ClosingDate = vacancy.ClosingDate.Value.AsDisplayDate();
            vm.EmployerDescription = vacancy.EmployerDescription;
            vm.EmployerName = vacancy.EmployerName;
            vm.EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl;
            vm.ExpectedDuration = vacancy.Wage.DurationUnit.Value.GetDisplayName().ToQuantity(vacancy.Wage.Duration.Value);
            vm.HoursPerWeek = $"{vacancy.Wage.WeeklyHours:0.##}";
            vm.Location = vacancy.EmployerLocation;
            vm.MapUrl = vacancy.EmployerLocation.HasGeocode
                ? _mapService.GetMapImageUrl(vacancy.EmployerLocation.Latitude.ToString(), vacancy.EmployerLocation.Longitude.ToString())
                : _mapService.GetMapImageUrl(vacancy.EmployerLocation?.Postcode);
            vm.NumberOfPositions = vacancy.NumberOfPositions.Value;
            vm.OutcomeDescription = vacancy.OutcomeDescription;
            vm.PossibleStartDate = vacancy.StartDate.Value.AsDisplayDate();
            vm.ProviderName = vacancy.TrainingProvider?.Name;
            vm.ProviderAddress = vacancy.TrainingProvider?.Address?.GetInlineAddress();
            vm.Qualifications = vacancy.Qualifications.SortQualifications(_qualificationsConfiguration.QualificationTypes).AsText();
            vm.ShortDescription = vacancy.ShortDescription;
            vm.Skills = vacancy.Skills ?? Enumerable.Empty<string>();
            vm.ThingsToConsider = vacancy.ThingsToConsider;
            vm.Title = vacancy.Title;
            vm.TrainingDescription = vacancy.TrainingDescription;
            vm.TrainingTitle = vacancy.Programme.Title;
            vm.TrainingType = vacancy.Programme.TrainingType?.GetDisplayName();
            vm.TrainingLevel = vacancy.Programme.LevelName;
            vm.VacancyDescription = vacancy.Description;
            vm.VacancyReferenceNumber = string.Empty;
            vm.WageInfo = vacancy.Wage.WageAdditionalInformation;
            vm.WageText = vacancy.Wage?.ToText(
                () => _wageService.GetNationalMinimumWageRange(vacancy.StartDate.Value),
                () => _wageService.GetApprenticeNationalMinimumWage(vacancy.StartDate.Value));
            vm.WorkingWeekDescription = vacancy.Wage.WorkingWeekDescription;
        }
    }
}
