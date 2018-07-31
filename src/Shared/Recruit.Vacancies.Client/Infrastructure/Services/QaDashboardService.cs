using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services
{
    public class QaDashboardService : IQaDashboardService
    {
        private readonly IVacancyReviewRepository _reviewRepository;
        private readonly IQueryStoreWriter _queryStoreWriter;
        private readonly ITimeProvider _timeProvider;

        public QaDashboardService(IVacancyReviewRepository reviewRepository, IQueryStoreWriter queryStoreWriter, ITimeProvider timeProvider)
        {
            _reviewRepository = reviewRepository;
            _queryStoreWriter = queryStoreWriter;
            _timeProvider = timeProvider;
        }

        public async Task RebuildQaDashboardAsync()
        {
            var activeReviews = await _reviewRepository.GetActiveAsync();

            var qaDashboard = new QaDashboard
            {
                TotalVacanciesForReview = activeReviews.Count,
                TotalVacanciesResubmitted = GetTotalVacanciesResubmittedCount(activeReviews),
                TotalVacanciesBrokenSla = GetTotalVacanciesBrokenSla(activeReviews),
                AllReviews = activeReviews.ToList()
            };

            await _queryStoreWriter.UpdateQaDashboardAsync(qaDashboard);
        }

        private int GetTotalVacanciesResubmittedCount(IEnumerable<VacancyReview> activeReviews)
        {
            return activeReviews
                .Where(r => r.SubmissionCount > 1)
                .Select(r => r.VacancyReference)
                .Distinct()
                .Count();
        }

        private int GetTotalVacanciesBrokenSla(IEnumerable<VacancyReview> activeReviews)
        {
            return activeReviews.Count(r => r.SlaDeadline.HasValue && r.SlaDeadline.Value < _timeProvider.Now);
        }
    }
}
