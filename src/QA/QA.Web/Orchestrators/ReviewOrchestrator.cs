using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Extensions;
using Esfa.Recruit.Qa.Web.ViewModels;
using Esfa.Recruit.Shared;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Services.MinimumWage;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Humanizer;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Qa.Web.Orchestrators
{
    public class ReviewOrchestrator
    {
        private const int MapImageWidth = 465;
        private const int MapImageHeight = 256;
        private readonly IQaVacancyClient _vacancyClient;
        private readonly IGeocodeImageService _mapService;
        private readonly QualificationsConfiguration _qualificationsConfiguration;
        private readonly IGetMinimumWages _wageService;

        public ReviewOrchestrator(
                    IQaVacancyClient vacancyClient, 
                    IGeocodeImageService mapService, 
                    IOptions<QualificationsConfiguration> qualificationsConfigOptions,
                    IGetMinimumWages wageService)
        {
            _vacancyClient = vacancyClient;
            _mapService = mapService;
            _qualificationsConfiguration = qualificationsConfigOptions.Value;
            _wageService = wageService;
        }

        public Task ApproveReviewAsync(Guid reviewId)
        {
            return _vacancyClient.ApproveReview(reviewId);
        }

        public async Task<ReviewViewModel> GetReviewViewModelAsync(Guid reviewId)
        {            
            var review = await _vacancyClient.GetVacancyReviewAsync(reviewId);

            if (review.Status == ReviewStatus.PendingReview)
            {
                review.Status = ReviewStatus.UnderReview;
                await _vacancyClient.UpdateVacancyReviewAsync(review);
            }

            var vacancy = await _vacancyClient.GetVacancyAsync(review.VacancyReference);
            var vm = await MapToViewModel(vacancy);
            
            return vm;
        }

        private async Task<ReviewViewModel> MapToViewModel(Vacancy vacancy)
        {
            var programme = await _vacancyClient.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId);

            var vm = new ReviewViewModel();

            vm.SubmittedByName = vacancy.SubmittedByUser.Name;
            vm.SubmittedByEmail = vacancy.SubmittedByUser.Email;
            vm.ApplicationInstructions = vacancy.ApplicationInstructions;
            vm.ApplicationUrl = vacancy.ApplicationUrl;
            vm.ContactName = vacancy.EmployerContactName;
            vm.ContactEmail = vacancy.EmployerContactEmail;
            vm.ContactTelephone = vacancy.EmployerContactPhone;
            vm.ClosingDate = vacancy.ClosingDate.Value.AsDisplayDate();
            vm.EmployerDescription = vacancy.EmployerDescription;
            vm.EmployerName = vacancy.EmployerName;
            vm.EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl;
            SetEmployerAddressElements(vm, vacancy);
            vm.NumberOfPositionsCaption =  $"{"position".ToQuantity(vacancy.NumberOfPositions.Value)} available";
            vm.OutcomeDescription = vacancy.OutcomeDescription;
            vm.PossibleStartDate = vacancy.StartDate.Value.AsDisplayDate();
            vm.ProviderName = vacancy.TrainingProvider.Name;
            vm.Qualifications = vacancy.Qualifications.SortQualifications(_qualificationsConfiguration.QualificationTypes).AsText();
            vm.ShortDescription = vacancy.ShortDescription;
            vm.Skills = vacancy.Skills ?? Enumerable.Empty<string>();
            vm.ThingsToConsider = vacancy.ThingsToConsider;
            vm.Title = vacancy.Title;
            vm.TrainingDescription = vacancy.TrainingDescription;
            vm.VacancyDescription = vacancy.Description;
            vm.VacancyReferenceNumber = vacancy.VacancyReference.ToString();
            vm.TrainingTitle = programme.Title;
            vm.TrainingType = programme.ApprenticeshipType.GetDisplayName();
            vm.TrainingLevel = programme == null ? null : programme.Level.GetDisplayName();
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