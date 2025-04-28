using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Humanizer;
using Microsoft.Extensions.Options;
using FeatureNames = Esfa.Recruit.Employer.Web.Configuration.FeatureNames;

namespace Esfa.Recruit.Employer.Web.Mappings
{
    public class DisplayVacancyViewModelMapper(
        IGeocodeImageService mapService,
        IOptions<ExternalLinksConfiguration> externalLinksOptions,
        IRecruitVacancyClient vacancyClient,
        IApprenticeshipProgrammeProvider apprenticeshipProgrammeProvider,
        IFeature feature)
    {
        private const int MapImageWidth = 465;
        private const int MapImageHeight = 256;
        private readonly ExternalLinksConfiguration _externalLinksConfiguration = externalLinksOptions.Value;

        public async Task MapFromVacancyAsync(DisplayVacancyViewModel vm, Vacancy vacancy)
        {
            ApprenticeshipStandard programme = null;
            if (int.TryParse(vacancy.ProgrammeId, out var standardId))
            {
                programme = await apprenticeshipProgrammeProvider.GetApprenticeshipStandardVacancyPreviewData(standardId);    
            }

            bool? hasOptedToAddQualifications = null;
            if (vacancy.HasOptedToAddQualifications == null)
            {
                if (vacancy.Qualifications != null && vacancy.Qualifications.Count != 0)
                {
                    hasOptedToAddQualifications = true;
                }
            }
            else
            {
                hasOptedToAddQualifications = vacancy.HasOptedToAddQualifications.Value;
            }
            

            var allQualifications = await vacancyClient.GetCandidateQualificationsAsync();

            vm.VacancyId = vacancy.Id;
            vm.EmployerAccountId = vacancy.EmployerAccountId;
            vm.AccountLegalEntityPublicHashedId = vacancy.AccountLegalEntityPublicHashedId;
            vm.Status = vacancy.Status;
            vm.ApplicationMethod = vacancy.ApplicationMethod;
            vm.ApplicationInstructions = vacancy.ApplicationInstructions;
            vm.ApplicationUrl = vacancy.ApplicationUrl;
            vm.CanDelete = vacancy.CanDelete;
            vm.CanSubmit = vacancy.CanSubmit && vacancy.Status != VacancyStatus.Review;
            vm.CanReview = vacancy.CanReview;
            vm.ClosingDate = (vacancy.ClosedDate ?? vacancy.ClosingDate)?.AsGdsDate();
            vm.EducationLevelName = programme != null ?
                EducationLevelNumberHelper.GetEducationLevelNameOrDefault(programme.EducationLevelNumber, programme.ApprenticeshipLevel) : null;
            vm.EmployerContactName = vacancy.EmployerContact?.Name;
            vm.EmployerContactEmail = vacancy.EmployerContact?.Email;
            vm.EmployerContactTelephone = vacancy.EmployerContact?.Phone;
            vm.EmployerDescription = await vacancyClient.GetEmployerDescriptionAsync(vacancy);
            vm.EmployerName = await vacancyClient.GetEmployerNameAsync(vacancy);
            vm.EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl;
            vm.EmployerAddressElements = [];
            vm.AvailableLocations = vacancy.EmployerLocations ?? [];
            vm.AvailableWhere = vacancy.EmployerLocationOption;
            vm.LocationInformation = vacancy.EmployerLocationInformation;
            vm.FindAnApprenticeshipUrl = _externalLinksConfiguration.FindAnApprenticeshipUrl;
            vm.IsAnonymous = vacancy.IsAnonymous;
            vm.NumberOfPositions = vacancy.NumberOfPositions?.ToString();
            vm.NumberOfPositionsCaption = vacancy.NumberOfPositions.HasValue
                ? $"{"position".ToQuantity(vacancy.NumberOfPositions.Value)} available"
                : null;
            if (feature.IsFeatureEnabled(FeatureNames.MultipleLocations))
            {
                vm.OrganisationName = vacancy.LegalEntityName;
            }
            vm.OutcomeDescription = vacancy.OutcomeDescription;
            vm.PossibleStartDate = vacancy.StartDate?.ToFullDateTimeString();
            vm.PostedDate = vacancy.CreatedDate?.AsGdsDate();
            vm.ProviderName = vacancy.TrainingProvider?.Name;
            vm.ProviderReviewFieldIndicators = vacancy.ProviderReviewFieldIndicators;
            vm.Qualifications = vacancy.Qualifications.SortQualifications(allQualifications).AsText().ToList();
            vm.QualificationsEssential = vacancy.Qualifications?.Where(c=>c.Weighting == QualificationWeighting.Essential).SortQualifications(allQualifications).AsText().ToList();
            vm.QualificationsDesired = vacancy.Qualifications?.Where(c=>c.Weighting == QualificationWeighting.Desired).SortQualifications(allQualifications).AsText().ToList();
            vm.HasOptedToAddQualifications = hasOptedToAddQualifications;
            vm.ShortDescription = vacancy.ShortDescription;
            vm.Skills = vacancy.Skills ?? Enumerable.Empty<string>();
            vm.ThingsToConsider = vacancy.ThingsToConsider;
            vm.Title = vacancy.Title;
            vm.TrainingDescription = vacancy.TrainingDescription;
            vm.AdditionalTrainingDescription = vacancy.AdditionalTrainingDescription;
            vm.VacancyDescription = vacancy.Description;
            vm.VacancyReferenceNumber = vacancy.VacancyReference.HasValue
                                        ? $"VAC{vacancy.VacancyReference.ToString()}"
                                        : string.Empty;
            vm.IsDisabilityConfident = vacancy.IsDisabilityConfident;
            vm.TransferredProviderName = vacancy.TransferInfo?.ProviderName;
            vm.TransferredOnDate = vacancy.TransferInfo?.TransferredDate.AsGdsDate();
            vm.EmployerNameOption = vacancy.EmployerNameOption;
            vm.AdditionalQuestion1 = vacancy.AdditionalQuestion1;
            vm.AdditionalQuestion2 = vacancy.AdditionalQuestion2;
            vm.HasSubmittedAdditionalQuestions = vacancy.HasSubmittedAdditionalQuestions;
            
            if (vacancy.EmployerLocation != null)
            {
                if (vacancy.EmployerLocation != null)
                    vm.MapUrl = MapImageHelper.GetEmployerLocationMapUrl(vacancy, mapService, MapImageWidth, MapImageHeight);

                vm.EmployerAddressElements = vacancy.EmployerAddressForDisplay();
            }

            if (vacancy.ProgrammeId != null && programme != null)
            {
                vm.TrainingTitle = programme.Title;
                vm.TrainingType = programme.ApprenticeshipType.GetDisplayName();
                vm.TrainingLevel = programme.ApprenticeshipLevel.GetDisplayName();
                vm.CourseCoreDuties = programme.CoreDuties;
                vm.CourseSkills = programme.Skills;
                vm.StandardPageUrl = programme.StandardPageUrl;
                vm.OverviewOfRole = programme.OverviewOfRole;
                vm.ApprenticeshipLevel = programme.ApprenticeshipLevel;
                vm.VacancyType = programme.ApprenticeshipType switch {
                    TrainingType.Foundation => VacancyType.Foundation,
                    _ => VacancyType.Apprenticeship
                };
            }

            if (vacancy.Wage != null)
            {
                vm.ExpectedDuration = (vacancy.Wage.DurationUnit.HasValue && vacancy.Wage.Duration.HasValue)
                    ? vacancy.Wage.DurationUnit.Value.GetDisplayName().ToLower().ToQuantity(vacancy.Wage.Duration.Value)
                    : null;
                vm.HoursPerWeek = $"{vacancy.Wage.WeeklyHours:0.##}";
                vm.WageInfo = vacancy.Wage.WageAdditionalInformation;
                vm.CompanyBenefitsInformation = vacancy.Wage.CompanyBenefitsInformation;
                vm.WageType = vacancy.Wage.WageType;
                vm.WageText = vacancy.StartDate.HasValue ? vacancy.Wage.ToText(vacancy.StartDate) : null;
                vm.WorkingWeekDescription = vacancy.Wage.WorkingWeekDescription;
            }
        }
    }
}