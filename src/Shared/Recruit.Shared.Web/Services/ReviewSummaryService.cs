using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Shared.Web.Services
{
    public class ReviewSummaryService : IReviewSummaryService
    {
        private readonly IEmployerVacancyClient _employerVacancyClient;
        private readonly ReviewFieldIndicatorMapper _fieldMappingsLookup;
        private readonly IQaVacancyClient _qaVacancyClient;

        public ReviewSummaryService(IEmployerVacancyClient employerVacancyClient, 
            ReviewFieldIndicatorMapper fieldMappingsLookup, IQaVacancyClient qaVacancyClient)
        {
            _employerVacancyClient = employerVacancyClient;
            _fieldMappingsLookup = fieldMappingsLookup;
            _qaVacancyClient = qaVacancyClient;
        }

        public async Task<ReviewSummaryViewModel> GetReviewSummaryViewModelAsync(long vacancyReference, 
            ReviewFieldMappingLookupsForPage reviewFieldIndicatorsForPage)
        {
            var review = await _employerVacancyClient.GetCurrentReferredVacancyReviewAsync(vacancyReference);

            return ConvertToReviewSummaryViewModel(reviewFieldIndicatorsForPage, review);
        }

        public async Task<ReviewSummaryViewModel> GetReviewSummaryViewModelAsync(Guid reviewId,
            ReviewFieldMappingLookupsForPage reviewFieldIndicatorsForPage)
        {
            var review = await _qaVacancyClient.GetVacancyReviewAsync(reviewId);

            return ConvertToReviewSummaryViewModel(reviewFieldIndicatorsForPage, review);
        }

        private ReviewSummaryViewModel ConvertToReviewSummaryViewModel(
            ReviewFieldMappingLookupsForPage reviewFieldIndicatorsForPage, VacancyReview review)
        {
            if (review != null)
            {
                var fieldIndicators =
                    _fieldMappingsLookup.MapFromFieldIndicators(reviewFieldIndicatorsForPage, review).ToList();

                return new ReviewSummaryViewModel
                {
                    HasBeenReviewed = true,
                    ReviewerComments = review.ManualQaComment,
                    FieldIndicators = fieldIndicators
                };
            }

            return new ReviewSummaryViewModel();
        }
    }
}
