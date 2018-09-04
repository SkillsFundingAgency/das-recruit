using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Services;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Providers
{
    public class NextVacancyReviewProvider : INextVacancyReviewProvider
    {
        private readonly ITimeProvider _timeProvider;
        private readonly IVacancyReviewRepository _vacancyReviewRepository;

        public NextVacancyReviewProvider(ITimeProvider timeProvider, IVacancyReviewRepository vacancyReviewRepository)
        {
            _timeProvider = timeProvider;   
            _vacancyReviewRepository = vacancyReviewRepository;
        }

        public async Task<VacancyReview> GetNextVacancyReviewAsync(string userId)
        {
            var assignationExpiryTime = _timeProvider.Now.AddHours(VacancyReview.AssignationTimeoutHours * -1);

            var assignedReviews = await _vacancyReviewRepository.GetByStatusAsync(ReviewStatus.UnderReview);

            //Get the oldest unexpired review assigned to the user
            var nextVacancyReview = assignedReviews.Where(r => 
                    r.ReviewedByUser.UserId == userId
                    && r.ReviewedDate >= assignationExpiryTime)
                .OrderBy(r => r.CreatedDate)
                .FirstOrDefault();

            if (nextVacancyReview != null)
                return nextVacancyReview;

            //Get the oldest expired assigned review
            nextVacancyReview = assignedReviews.Where(r =>
                    r.ReviewedDate < assignationExpiryTime)
                .OrderBy(r => r.CreatedDate)
                .FirstOrDefault();

            if (nextVacancyReview != null)
                return nextVacancyReview;

            //Get the oldest unassigned review
            var prendingReviews = await _vacancyReviewRepository.GetByStatusAsync(ReviewStatus.PendingReview);
            nextVacancyReview = prendingReviews
                .OrderBy(r => r.CreatedDate)
                .FirstOrDefault();

            return nextVacancyReview;
        }
    }
}
