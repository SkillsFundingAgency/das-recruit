using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Shared.Web.Services
{
    public class ReviewSummaryService(
        IRecruitVacancyClient vacancyClient,
        ReviewFieldIndicatorMapper fieldMappingsLookup)
        : IReviewSummaryService
    {
        public async Task<ReviewSummaryViewModel> GetReviewSummaryViewModelAsync(long vacancyReference, 
            ReviewFieldMappingLookupsForPage reviewFieldIndicatorsForPage)
        {
            var review = await vacancyClient.GetCurrentReferredVacancyReviewAsync(vacancyReference);

            return ConvertToReviewSummaryViewModel(reviewFieldIndicatorsForPage, review);
        }

        private ReviewSummaryViewModel ConvertToReviewSummaryViewModel(
            ReviewFieldMappingLookupsForPage reviewFieldIndicatorsForPage, VacancyReview review)
        {
            if (review != null)
            {
                var fieldIndicators =
                    fieldMappingsLookup.MapFromFieldIndicators(reviewFieldIndicatorsForPage, review).ToList();

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
