using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Events.Interfaces;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers
{
    public class UpdateVacancyApplicationsOnApplicationReviewChange :
        INotificationHandler<ApplicationReviewCreatedEvent>,
        INotificationHandler<ApplicationReviewWithdrawnEvent>,
        INotificationHandler<ApplicationReviewedEvent>
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IApplicationReviewRepository _applicationReviewRepository;
        private readonly IQueryStoreWriter _writer;
        private readonly ILogger<UpdateVacancyApplicationsOnApplicationReviewChange> _logger;

        public UpdateVacancyApplicationsOnApplicationReviewChange(IVacancyRepository vacancyRepository, IApplicationReviewRepository applicationReviewRepository, IQueryStoreWriter writer, ILogger<UpdateVacancyApplicationsOnApplicationReviewChange> logger)
        {
            _vacancyRepository = vacancyRepository;
            _applicationReviewRepository = applicationReviewRepository;
            _writer = writer;
            _logger = logger;
        }

        public Task Handle(ApplicationReviewCreatedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(ApplicationReviewWithdrawnEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(ApplicationReviewedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        private async Task Handle(IApplicationReviewEvent notification)
        {
            _logger.LogInformation("Handling {notificationType} for vacancyReference: {vacancyReference}", notification.GetType().Name, notification?.VacancyReference);

            var vacancy = await _vacancyRepository.GetVacancyAsync(notification.VacancyReference);
            var vacancyApplicationReviews = await _applicationReviewRepository.GetForVacancyAsync<ApplicationReview>(vacancy.VacancyReference.Value);

            var vacancyApplications = new VacancyApplications
            {
                VacancyReference = vacancy.VacancyReference.Value,
                Applications = vacancyApplicationReviews.Select(MapToVacancyApplication).ToList()
            };

            await _writer.UpdateVacancyApplicationsAsync(vacancyApplications);
        }

        private VacancyApplication MapToVacancyApplication(ApplicationReview review)
        {
            var projection = new VacancyApplication
            {
                Status = review.Status,
                SubmittedDate = review.SubmittedDate,
                ApplicationReviewId = review.Id,
                IsWithdrawn = review.IsWithdrawn,
                CandidateName = null,
                DisabilityStatus = ApplicationReviewDisabilityStatus.Unknown
            };

            if (review.IsWithdrawn == false)
            {
                projection.CandidateName = review.Application.FullName;
                projection.DisabilityStatus = review.Application.DisabilityStatus ?? ApplicationReviewDisabilityStatus.Unknown;
            }

            return projection;
        }
    }
}
