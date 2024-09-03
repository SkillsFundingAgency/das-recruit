using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Humanizer;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Provider.Web.Mappings
{
    public class DisplayVacancyViewModelMapper
    {
        private const int MapImageWidth = 465;
        private const int MapImageHeight = 256;
        private readonly IGeocodeImageService _mapService;
        private readonly ExternalLinksConfiguration _externalLinksConfiguration;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IProviderVacancyClient _client;
        private readonly IFeature _feature;

        public DisplayVacancyViewModelMapper(
                IGeocodeImageService mapService,
                IOptions<ExternalLinksConfiguration> externalLinksOptions,
                IRecruitVacancyClient vacancyClient,
                IProviderVacancyClient client,
                IFeature feature)
        {
            _mapService = mapService;
            _externalLinksConfiguration = externalLinksOptions.Value;
            _vacancyClient = vacancyClient;
            _client = client;
            _feature = feature;
        }

        public async Task MapFromVacancyAsync(DisplayVacancyViewModel vm, Vacancy vacancy)
        {
            var programme = vacancy.VacancyType.GetValueOrDefault() == VacancyType.Apprenticeship ? await _vacancyClient.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId) : null;
            var route = vacancy.VacancyType.GetValueOrDefault() == VacancyType.Traineeship ? await _vacancyClient.GetRoute(vacancy.RouteId) : null;
            var employer = await _client.GetProviderEmployerVacancyDataAsync(vacancy.TrainingProvider.Ukprn.Value, vacancy.EmployerAccountId);

            var allQualifications = await _vacancyClient.GetCandidateQualificationsAsync();

            vm.Status = vacancy.Status;
            vm.AccountName = employer.Name;
            vm.ApplicationMethod = vacancy.ApplicationMethod;
            vm.ApplicationInstructions = vacancy.ApplicationInstructions;
            vm.ApplicationUrl = vacancy.ApplicationUrl;
            vm.CanDelete = vacancy.CanDelete;
            vm.CanSubmit = vacancy.CanSubmit;
            vm.IsSentForReview = vacancy.Status == VacancyStatus.Review;
            vm.ClosingDate = (vacancy.ClosedDate ?? vacancy.ClosingDate)?.AsGdsDate();
            vm.PostedDate = vacancy.CreatedDate?.AsGdsDate();
            vm.EducationLevelName = programme?.EducationLevelNumber != null ? EducationLevelNumberHelper.GetEducationLevelNameOrDefault(programme.EducationLevelNumber, programme.ApprenticeshipLevel) : "";
            vm.EmployerDescription = vacancy.EmployerDescription;
            vm.EmployerName = await _vacancyClient.GetEmployerNameAsync(vacancy);
            vm.EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl;
            vm.EmployerAddressElements = Enumerable.Empty<string>();
            vm.EmployerRejectedReason = vacancy.EmployerRejectedReason;
            vm.EmployerReviewFieldIndicators = vacancy.EmployerReviewFieldIndicators;
            vm.FindAnApprenticeshipUrl = _externalLinksConfiguration.FindAnApprenticeshipUrl;
            vm.FindATraineeshipUrl = _externalLinksConfiguration.FindATraineeshipUrl;
            vm.IsAnonymous = vacancy.IsAnonymous;
            vm.NumberOfPositions = vacancy.NumberOfPositions?.ToString();
            vm.NumberOfPositionsCaption = vacancy.NumberOfPositions.HasValue
                ? $"{"position".ToQuantity(vacancy.NumberOfPositions.Value)} available"
                : null;
            vm.OutcomeDescription = vacancy.OutcomeDescription;
            vm.PossibleStartDate = vacancy.StartDate?.AsGdsDate();
            vm.ProviderContactName = vacancy.ProviderContact?.Name;
            vm.ProviderContactEmail = vacancy.ProviderContact?.Email;
            vm.ProviderContactTelephone = vacancy.ProviderContact?.Phone;
            vm.ProviderName = vacancy.TrainingProvider?.Name;
            vm.Qualifications = vacancy.Qualifications.SortQualifications(allQualifications).AsText(_feature.IsFeatureEnabled(FeatureNames.FaaV2Improvements));
            vm.HasOptedToAddQualifications = !_feature.IsFeatureEnabled(FeatureNames.FaaV2Improvements) ? true : vacancy.HasOptedToAddQualifications;
            vm.ShortDescription = vacancy.ShortDescription;
            vm.Skills = vacancy.Skills ?? Enumerable.Empty<string>();
            vm.ThingsToConsider = vacancy.ThingsToConsider;
            vm.Title = vacancy.Title;
            vm.TrainingDescription = vacancy.TrainingDescription;
            vm.AdditionalTrainingDescription = vacancy.AdditionalTrainingDescription;
            vm.VacancyDescription = vacancy.Description;
            vm.VacancyReferenceNumber = vacancy.VacancyReference.HasValue
                                        ? $"VAC{vacancy.VacancyReference}"
                                        : string.Empty;
            vm.IsDisabilityConfident = vacancy.IsDisabilityConfident;
            vm.AccountLegalEntityPublicHashedId = vacancy.AccountLegalEntityPublicHashedId;
            vm.EmployerNameOption = vacancy.EmployerNameOption;
            vm.AdditionalQuestion1 = vacancy.AdditionalQuestion1;
            vm.AdditionalQuestion2 = vacancy.AdditionalQuestion2;
            vm.HasSubmittedAdditionalQuestions = vacancy.HasSubmittedAdditionalQuestions;

            if (vacancy.EmployerLocation != null)
            {
                vm.MapUrl = MapImageHelper.GetEmployerLocationMapUrl(vacancy, _mapService, MapImageWidth, MapImageHeight);

                vm.EmployerAddressElements = vacancy.EmployerAddressForDisplay();
            }

            if (programme != null)
            {
                vm.TrainingTitle = programme.Title;
                vm.TrainingType = programme.ApprenticeshipType.GetDisplayName();
                vm.TrainingLevel = programme.ApprenticeshipLevel.GetDisplayName();
            }

            if (vacancy.Wage != null)
            {
                vm.ExpectedDuration = (vacancy.Wage.DurationUnit.HasValue && vacancy.Wage.Duration.HasValue)
                    ? vacancy.Wage.DurationUnit.Value.GetDisplayName().ToQuantity(vacancy.Wage.Duration.Value)
                    : null;
                vm.HoursPerWeek = $"{vacancy.Wage.WeeklyHours:0.##} hours a week";
                vm.WageInfo = vacancy.Wage.WageAdditionalInformation;
                vm.CompanyBenefitsInformation = vacancy.Wage.CompanyBenefitsInformation;
                vm.WageType = vacancy.Wage.WageType;
                vm.WageText = vacancy.StartDate.HasValue ? vacancy.Wage.ToText(vacancy.StartDate) : null;
                vm.WorkingWeekDescription = vacancy.Wage.WorkingWeekDescription;
            }

            if (route != null)
            {
                vm.RouteId = route.Id;
                vm.RouteTitle = route.Route;
            }

            vm.WorkExperience = vacancy.WorkExperience;

            vm.VacancyType = vacancy.VacancyType;
        }
    }
}