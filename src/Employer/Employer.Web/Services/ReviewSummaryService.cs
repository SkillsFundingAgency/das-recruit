using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Services
{
    public class ReviewSummaryService : IReviewSummaryService
    {
        private readonly IEmployerVacancyClient _client;
        public ReviewSummaryService(IEmployerVacancyClient client)
        {
            _client = client;
        }

        public async Task<ReviewSummaryViewModel> GetReviewSummaryViewModel(long vacancyReference, IEnumerable<ReviewFieldIndicatorViewModel> reviewFieldIndicatorsForPage)
        {
            ReviewSummaryViewModel vm;
            var review = await _client.GetCurrentReferredVacancyReviewAsync(vacancyReference);

            if (review != null)
            {
                var fieldIndicators = ReviewFieldIndicatorMapper.MapFromFieldIndicators(reviewFieldIndicatorsForPage, review.ManualQaFieldIndicators, review.AutomatedQaOutcome).ToList();

                vm = new ReviewSummaryViewModel
                {
                    CanDisplayReviewHeader = true,
                    ReviewerComments = review.ManualQaComment,
                    FieldIndicators = fieldIndicators
                };
            }
            else
            {
                vm = new ReviewSummaryViewModel { CanDisplayReviewHeader = false };
            }

            return vm;
        }
    }
}
