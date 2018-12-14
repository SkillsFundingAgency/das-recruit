using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Vacancies.Client.Application.Services.NextVacancyReview
{
    public class NextVacancyReviewService : INextVacancyReviewService
    {
        private readonly ITimeProvider _timeProvider;
        private readonly IVacancyReviewQuery _vacancyReviewQuery;
        private readonly NextVacancyReviewServiceConfiguration _config;

        public NextVacancyReviewService(IOptions<NextVacancyReviewServiceConfiguration> config, ITimeProvider timeProvider, IVacancyReviewQuery vacancyReviewQuery)
        {
            _timeProvider = timeProvider;   
            _vacancyReviewQuery = vacancyReviewQuery;
            _config = config.Value;
        }

        public async Task<VacancyReview> GetNextVacancyReviewAsync(string userId)
        {
            var assignationExpiryTime = _timeProvider.Now.AddMinutes(_config.VacancyReviewAssignationTimeoutMinutes * -1);

            var assignedReviews = await _vacancyReviewQuery.GetByStatusAsync(ReviewStatus.UnderReview);

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
            var pendingReviews = await _vacancyReviewQuery.GetByStatusAsync(ReviewStatus.PendingReview);
            nextVacancyReview = pendingReviews
                .OrderBy(r => r.CreatedDate)
                .FirstOrDefault();

            return nextVacancyReview;
        }

        public DateTime GetExpiredAssignationDateTime()
        {
            return _timeProvider.Now.AddMinutes(_config.VacancyReviewAssignationTimeoutMinutes * -1);
        }

        public bool VacancyReviewCanBeAssigned(ReviewStatus reviewStatus, DateTime? reviewedDate)
        {
            if (reviewStatus == ReviewStatus.PendingReview)
                return true;

            return reviewedDate < GetExpiredAssignationDateTime();
        }
    }
}
