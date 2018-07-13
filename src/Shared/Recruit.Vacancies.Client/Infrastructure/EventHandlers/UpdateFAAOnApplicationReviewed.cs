using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.FAA;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers
{
    public class UpdateFaaOnApplicationReviewed :
        INotificationHandler<ApplicationReviewedEvent>
    {
        private readonly IFaaService _faaService;

        public UpdateFaaOnApplicationReviewed(IFaaService faaService)
        {
            _faaService = faaService;
        }

        public Task Handle(ApplicationReviewedEvent notification, CancellationToken cancellationToken)
        {
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
