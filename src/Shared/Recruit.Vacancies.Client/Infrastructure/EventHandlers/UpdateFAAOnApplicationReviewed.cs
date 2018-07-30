using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.FAA;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers
{
    public class UpdateFaaOnApplicationReviewed :
        INotificationHandler<ApplicationReviewedEvent>
    {
        private readonly IFaaService _faaService;
        private readonly ILogger<UpdateFaaOnApplicationReviewed> _logger;

        public UpdateFaaOnApplicationReviewed(IFaaService faaService, ILogger<UpdateFaaOnApplicationReviewed> logger)
        {
            _faaService = faaService;
            _logger = logger;
        }

        public Task Handle(ApplicationReviewedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling {notificationType}", notification.GetType().Name);

            if (notification.Status != ApplicationReviewStatus.Successful 
                && notification.Status != ApplicationReviewStatus.Unsuccessful)
            {
                return Task.CompletedTask;
            }

            var message = new FaaApplicationStatusSummary
            {
                VacancyReference = (int)notification.VacancyReference,
                UnsuccessfulReason = notification.CandidateFeedback,
                CandidateId = notification.CandidateId,
                IsRecruitVacancy = true,
                ApplicationStatus = notification.Status == ApplicationReviewStatus.Successful 
                    ? FaaApplicationStatus.Successful 
                    : FaaApplicationStatus.Unsuccessful
            };

            return _faaService.PublishApplicationStatusSummaryAsync(message);
        }
    }
}
