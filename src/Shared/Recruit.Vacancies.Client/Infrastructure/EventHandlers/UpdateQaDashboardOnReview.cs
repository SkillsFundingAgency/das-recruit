using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers
{
    public class UpdateQaDashboardOnReview : INotificationHandler<VacancyReviewCreatedEvent>,
                                             INotificationHandler<VacancyReviewApprovedEvent>,
                                             INotificationHandler<VacancyReviewReferredEvent>
    {
        private readonly IVacancyReviewRepository _reviewRepository;
        private readonly IQueryStoreWriter _queryStoreWriter;
        private readonly ILogger<UpdateQaDashboardOnReview> _logger;

        public UpdateQaDashboardOnReview(IVacancyReviewRepository reviewRepository, IQueryStoreWriter queryStoreWriter, ILogger<UpdateQaDashboardOnReview> logger)
        {
            _reviewRepository = reviewRepository;
            _queryStoreWriter = queryStoreWriter;
            _logger = logger;
        }

        public Task Handle(VacancyReviewCreatedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(VacancyReviewApprovedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(VacancyReviewReferredEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        private async Task Handle(IVacancyReviewEvent notification)
        {
            _logger.LogInformation("Handling {notificationType}", notification.GetType().Name);

            var activeReviews = await _reviewRepository.GetActiveAsync();

            var qaDashboard = new QaDashboard
            {
                TotalVacanciesForReview = activeReviews.Count,
                TotalVacanciesResubmitted = GetTotalVacanciesResubmittedCount(activeReviews),
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
    }
}
