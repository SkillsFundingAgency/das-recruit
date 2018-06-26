using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class ApplicationReviewCommandHandler : IRequestHandler<ApplicationReviewSuccessfulCommand>
    {
        private readonly ILogger<ApplicationReviewCommandHandler> _logger;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IApplicationReviewRepository _applicationReviewRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly IMessaging _messaging;

        public ApplicationReviewCommandHandler(
            ILogger<ApplicationReviewCommandHandler> logger,
            IVacancyRepository vacancyRepository,
            IApplicationReviewRepository applicationReviewRepository,
            ITimeProvider timeProvider,
            IMessaging messaging)
        {
            _logger = logger;
            _vacancyRepository = vacancyRepository;
            _applicationReviewRepository = applicationReviewRepository;
            _timeProvider = timeProvider;
            _messaging = messaging;
        }

        public async Task Handle(ApplicationReviewSuccessfulCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Setting application review:{applicationReviewId} to successful", message.ApplicationReviewId);

            var applicationReview = await _applicationReviewRepository.GetAsync(message.ApplicationReviewId);
            var vacancy = await _vacancyRepository.GetVacancyAsync(applicationReview.VacancyReference);

            applicationReview.Status = ApplicationReviewStatus.Successful;
            applicationReview.StatusUpdatedDate = _timeProvider.Now;
            applicationReview.StatusUpdatedBy = message.User;

            await _applicationReviewRepository.UpdateAsync(applicationReview);

            await _messaging.PublishEvent(new ApplicationReviewSuccessfulEvent
            {
                SourceCommandId = message.CommandId.ToString(),
                EmployerAccountId = applicationReview.EmployerAccountId,
                VacancyId = vacancy.Id
            });
        }
    }
}
