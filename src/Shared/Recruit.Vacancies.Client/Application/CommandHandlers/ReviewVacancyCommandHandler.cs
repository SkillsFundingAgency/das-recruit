using System;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Events;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class ReviewVacancyCommandHandler : IRequestHandler<ReviewVacancyCommand>
    {
        public const string VacancyNotFoundExceptionMessageFormat = "Vacancy {0} not found";
        public const string InvalidStateExceptionMessageFormat = "Unable to review vacancy {0} due to vacancy having a status of {1}.";
        public const string InvalidOwnerExceptionMessageFormat = "The vacancy {0} owner has changed from {1} to {2} and hence cannot be reviewed.";
        public const string MissingReferenceNumberExceptionMessageFormat = "Cannot review vacancy {0} without a vacancy reference number";
        private readonly ILogger<SubmitVacancyCommandHandler> _logger;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;
        private readonly IEmployerService _employerService;

        public ReviewVacancyCommandHandler(
            ILogger<SubmitVacancyCommandHandler> logger,
            IVacancyRepository vacancyRepository, 
            IMessaging messaging, 
            ITimeProvider timeProvider,
            IEmployerService employerService)
        {
            _logger = logger;
            _vacancyRepository = vacancyRepository;
            _messaging = messaging;
            _timeProvider = timeProvider;
            _employerService = employerService;
        }

        public async Task Handle(ReviewVacancyCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Reviewing vacancy {vacancyId}.", message.VacancyId);

            var vacancy = await _vacancyRepository.GetVacancyAsync(message.VacancyId);
            
            if (vacancy == null) 
                throw new ArgumentException(string.Format(VacancyNotFoundExceptionMessageFormat, message.VacancyId));

            if (vacancy.VacancyReference.HasValue == false)
                throw new InvalidOperationException(string.Format(MissingReferenceNumberExceptionMessageFormat, vacancy.Id));

            if(vacancy.CanSubmit == false)
                throw new InvalidOperationException(string.Format(InvalidStateExceptionMessageFormat, vacancy.Id, vacancy.Status));

            if(vacancy.OwnerType != message.SubmissionOwner)
                throw new InvalidOperationException(string.Format(InvalidOwnerExceptionMessageFormat, vacancy.Id, message.SubmissionOwner, vacancy.OwnerType));

            var now = _timeProvider.Now;

            if(!string.IsNullOrEmpty(message.EmployerDescription))
                vacancy.EmployerDescription = message.EmployerDescription;

            vacancy.EmployerName = await _employerService.GetEmployerNameAsync(vacancy);

            vacancy.Status = VacancyStatus.Review;
            vacancy.ReviewDate = now;
            vacancy.ReviewByUser = message.User;
            vacancy.LastUpdatedDate = now;
            vacancy.LastUpdatedByUser = message.User;

            await _vacancyRepository.UpdateAsync(vacancy);

            await _messaging.PublishEvent(new VacancyReviewedEvent
            {
                EmployerAccountId = vacancy.EmployerAccountId,
                VacancyId = vacancy.Id,
                VacancyReference = vacancy.VacancyReference.Value
            });
        }
    }
}
