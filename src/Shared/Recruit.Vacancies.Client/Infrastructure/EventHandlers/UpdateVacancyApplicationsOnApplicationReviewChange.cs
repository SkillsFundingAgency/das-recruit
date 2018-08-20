using System;
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

        public Task Handle(ApplicationReviewedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        private async Task Handle(IApplicationReviewEvent notification)
        {
            _logger.LogInformation("Handling {notificationType} for vacancyId: {vacancyId}", notification.GetType().Name, notification?.VacancyId);

            var vacancy = await _vacancyRepository.GetVacancyAsync(notification.VacancyId);
            var vacancyApplicationReviews = await _applicationReviewRepository.GetForVacancyAsync<ApplicationReview>(vacancy.VacancyReference.Value);

            var vacancyApplications = new VacancyApplications
            {
                VacancyReference = vacancy.VacancyReference.Value,
                Applications = vacancyApplicationReviews.Select(r => new VacancyApplication
                {
                    Status = r.Status,
                    SubmittedDate = r.SubmittedDate,
                    ApplicationReviewId = r.Id,
                    CandidateName = r.Application.FullName,
                    DisabilityStatus = r.Application.DisabilityStatus ?? ApplicationReviewDisabilityStatus.Unknown
                }).ToList()
            };

            await _writer.UpdateVacancyApplicationsAsync(vacancyApplications);
        }
    }
}
