using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Vacancies.Client.Application.Services.NextVacancyReview
{
    public class NextVacancyReviewService : INextVacancyReviewService
    {
        private readonly ITimeProvider _timeProvider;
        private readonly IVacancyReviewRepository _vacancyReviewRepository;
        private readonly NextVacancyReviewServiceConfiguration _config;

        public NextVacancyReviewService(IOptions<NextVacancyReviewServiceConfiguration> config, ITimeProvider timeProvider, IVacancyReviewRepository vacancyReviewRepository)
        {
            _timeProvider = timeProvider;   
            _vacancyReviewRepository = vacancyReviewRepository;
            _config = config.Value;
        }

        public async Task<VacancyReview> GetNextVacancyReviewAsync(string userId)
        {
            var assignationExpiryTime = _timeProvider.Now.AddMinutes(_config.VacancyReviewAssignationTimeoutMinutes * -1);

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

        public DateTime GetExpiredAssignationDateTime()
        {
            return _timeProvider.Now.AddMinutes(_config.VacancyReviewAssignationTimeoutMinutes * -1);
        }

        public bool VacancyReviewCanBeAssigned(VacancyReview review)
        {
            if (review.Status == ReviewStatus.PendingReview)
                return true;

            return review.ReviewedDate < GetExpiredAssignationDateTime();
        }
    }
}
