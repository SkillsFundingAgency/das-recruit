using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Services
{
    public class ReviewSummaryService : IReviewSummaryService
    {
        private readonly IEmployerVacancyClient _client;
        private readonly ReviewFieldIndicatorMapper _fieldMappingsLookup;

        public ReviewSummaryService(IEmployerVacancyClient client, ReviewFieldIndicatorMapper fieldMappingsLookup)
        {
            _client = client;
            _fieldMappingsLookup = fieldMappingsLookup;
        }

        public async Task<ReviewSummaryViewModel> GetReviewSummaryViewModel(long vacancyReference, ReviewFieldMappingLookupsForPage reviewFieldIndicatorsForPage)
        {
            ReviewSummaryViewModel vm;
            var review = await _client.GetCurrentReferredVacancyReviewAsync(vacancyReference);

            if (review != null)
            {
                var fieldIndicators = _fieldMappingsLookup.MapFromFieldIndicators(reviewFieldIndicatorsForPage, review).ToList();

                vm = new ReviewSummaryViewModel
                {
                    HasBeenReviewed = true,
                    ReviewerComments = review.ManualQaComment,
                    FieldIndicators = fieldIndicators
                };
            }
            else
            {
                vm = new ReviewSummaryViewModel { HasBeenReviewed = false };
            }

            return vm;
        }
    }
}
