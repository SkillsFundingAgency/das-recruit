using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Extensions;
using Esfa.Recruit.Qa.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Humanizer;

namespace Esfa.Recruit.Qa.Web.Orchestrators
{
    public class ReviewOrchestrator
    {
        private readonly IQaVacancyClient _vacancyClient;

        public ReviewOrchestrator(IQaVacancyClient vacancyClient)
        {
            _vacancyClient = vacancyClient;
        }

        public Task ApproveReviewAsync(Guid reviewId)
        {
            return _vacancyClient.ApproveReview(reviewId);
        }

        public async Task<ReviewViewModel> GetReviewViewModelAsync(Guid reviewId)
        {            
            var review = await _vacancyClient.GetVacancyReviewAsync(reviewId);
            
            // TODO: LWA - Should this be in the client i.e. StartReview()
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

            var vm = new ReviewViewModel
            {
                ApplicationInstructions = vacancy.ApplicationInstructions,
                ApplicationUrl = vacancy.ApplicationUrl,
                ContactName = vacancy.EmployerContactName,
                ContactEmail = vacancy.EmployerContactEmail,
                ContactTelephone = vacancy.EmployerContactPhone,
                ClosingDate = vacancy.ClosingDate.Value.AsDisplayDate(),
                EmployerDescription = vacancy.EmployerDescription,
                EmployerName = vacancy.EmployerName,
                EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl,
                EmployerAddressElements = Enumerable.Empty<string>(), // TODO: LWA - Build the address
                NumberOfPositions = vacancy.NumberOfPositions.ToString(),
                NumberOfPositionsCaption =  $"{"position".ToQuantity(vacancy.NumberOfPositions.Value)} available",
                OutcomeDescription = vacancy.OutcomeDescription,
                PossibleStartDate = vacancy.StartDate.Value.AsDisplayDate(),
                ProviderName = vacancy.TrainingProvider.Name,
                //Qualifications = vacancy.Qualifications.SortQualifications(_qualificationsConfiguration.QualificationTypes).AsText();
                ShortDescription = vacancy.ShortDescription,
                //Skills = vacancy.Skills ?? Enumerable.Empty<string>();
                ThingsToConsider = vacancy.ThingsToConsider,
                Title = vacancy.Title,
                TrainingDescription = vacancy.TrainingDescription,
                VacancyDescription = vacancy.Description,
                VacancyReferenceNumber = vacancy.VacancyReference.ToString(),
                TrainingTitle = programme.Title,
                TrainingType = programme.ApprenticeshipType.GetDisplayName(),
                TrainingLevel = programme == null ? null : programme.Level.GetDisplayName(),
                ExpectedDuration = (vacancy.Wage.DurationUnit.HasValue && vacancy.Wage.Duration.HasValue)
                    ? vacancy.Wage.DurationUnit.Value.GetDisplayName().ToQuantity(vacancy.Wage.Duration.Value)
                    : null,
                HoursPerWeek = $"{vacancy.Wage.WeeklyHours:0.##}",
                WageInfo = vacancy.Wage.WageAdditionalInformation,
                // WageText = vacancy.StartDate.HasValue
                //     ? vacancy.Wage.ToText(
                //         () => _wageService.GetNationalMinimumWageRange(vacancy.StartDate.Value),
                //         () => _wageService.GetApprenticeNationalMinimumWage(vacancy.StartDate.Value))
                //     : null;
                WorkingWeekDescription = vacancy.Wage.WorkingWeekDescription
            };

            return vm;
        }


    }
}