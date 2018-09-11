using System;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class CreateApplicationReviewCommandHandler : IRequestHandler<CreateApplicationReviewCommand>
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly ILogger<CreateApplicationReviewCommandHandler> _logger;
        private readonly ITimeProvider _timeProvider;
        private readonly IApplicationReviewRepository _applicationReviewRepository;
        private readonly IMessaging _messaging;

        public CreateApplicationReviewCommandHandler(IVacancyRepository vacancyRepository, IApplicationReviewRepository applicationReviewRepository, ILogger<CreateApplicationReviewCommandHandler> logger, ITimeProvider timeProvider, IMessaging messaging)
        {
            _logger = logger;
            _vacancyRepository = vacancyRepository;
            _applicationReviewRepository = applicationReviewRepository;
            _timeProvider = timeProvider;
            _messaging = messaging;
        }

        public async Task Handle(CreateApplicationReviewCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Submitting application for vacancyId: {vacancyReference} for candidateId: {candidateId}", message.Application.VacancyReference, message.Application.CandidateId);

            var vacancy = await _vacancyRepository.GetVacancyAsync(message.Application.VacancyReference);

            var review = new ApplicationReview
            {
                Id = Guid.NewGuid(),
                VacancyReference = vacancy.VacancyReference.Value,
                Application = message.Application,
                CandidateId = message.Application.CandidateId,
                CreatedDate = _timeProvider.Now,
                EmployerAccountId = vacancy.EmployerAccountId,
                Status = ApplicationReviewStatus.New,
                SubmittedDate = message.Application.ApplicationDate
            };

            await _applicationReviewRepository.CreateAsync(review);

            await _messaging.PublishEvent(new ApplicationReviewCreatedEvent
            {
                SourceCommandId = message.CommandId.ToString(),
                EmployerAccountId = vacancy.EmployerAccountId,
                VacancyId = vacancy.Id
            });
        }
    }
}
