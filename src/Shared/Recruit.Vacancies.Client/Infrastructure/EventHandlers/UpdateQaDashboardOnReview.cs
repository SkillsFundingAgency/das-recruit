using System;
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
        private readonly IQueryStoreWriter _queryStoryWriter;
        private readonly ILogger<UpdateQaDashboardOnReview> _logger;

        public UpdateQaDashboardOnReview(IVacancyReviewRepository reviewRepository, IQueryStoreWriter queryStoryWriter, ILogger<UpdateQaDashboardOnReview> logger)
        {
            _reviewRepository = reviewRepository;
            _queryStoryWriter = queryStoryWriter;
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
                AllReviews = activeReviews.ToList()
            };
            
            await _queryStoryWriter.UpdateQaDashboardAsync(qaDashboard);
        }
    }
}
