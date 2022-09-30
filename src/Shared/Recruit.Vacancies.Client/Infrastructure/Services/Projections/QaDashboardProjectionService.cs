using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections
{
    public class QaDashboardProjectionService : IQaDashboardProjectionService
    {
        private readonly IVacancyReviewQuery _reviewQuery;
        private readonly IQueryStoreWriter _queryStoreWriter;
        private readonly ITimeProvider _timeProvider;

        public QaDashboardProjectionService(IVacancyReviewQuery reviewQuery, IQueryStoreWriter queryStoreWriter, ITimeProvider timeProvider)
        {
            _reviewQuery = reviewQuery;
            _queryStoreWriter = queryStoreWriter;
            _timeProvider = timeProvider;
        }

        public async Task RebuildQaDashboardAsync()
        {
            var activeReviews = await _reviewQuery.GetActiveAsync<VacancyReviewSummary>();

            activeReviews = activeReviews.OrderByDescending(x => x.CreatedDate).ToList();

            var qaDashboard = new QaDashboard
            {
                TotalVacanciesForReview = activeReviews.Count,
                TotalVacanciesResubmitted = GetTotalVacanciesResubmittedCount(activeReviews),
                TotalVacanciesBrokenSla = GetTotalVacanciesBrokenSla(activeReviews),
                TotalVacanciesSubmittedTwelveTwentyFourHours = activeReviews.Count(c=>
                    c.SubmissionCount == 1 
                    && c.CreatedDate.HasValue 
                    && ((_timeProvider.Now - c.CreatedDate.Value).TotalHours >= 12 && (_timeProvider.Now - c.CreatedDate.Value).TotalHours < 24))
            };

            await _queryStoreWriter.UpdateQaDashboardAsync(qaDashboard);
        }

        private int GetTotalVacanciesResubmittedCount(IEnumerable<VacancyReviewSummary> activeReviews)
        {
            return activeReviews
                .Where(r => r.SubmissionCount > 1)
                .Select(r => r.VacancyReference)
                .Distinct()
                .Count();
        }

        private int GetTotalVacanciesBrokenSla(IEnumerable<VacancyReviewSummary> activeReviews)
        {
            return activeReviews.Count(r => r.SlaDeadline.HasValue && r.SlaDeadline.Value < _timeProvider.Now);
        }
    }
}
