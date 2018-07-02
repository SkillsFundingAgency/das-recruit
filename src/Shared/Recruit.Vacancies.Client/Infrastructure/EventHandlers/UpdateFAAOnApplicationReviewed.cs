using System;
using System.Collections.Generic;
using System.Text;
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
            var message = new FaaApplicationStatusSummary
            {
                VacancyReference = (int)notification.VacancyReference,
                UnsuccessfulReason = notification.CandidateFeedback,
                ApplicationStatus = GetApplicationStatus(notification.Status),
                CandidateId = notification.CandidateId,
                IsRecruitVacancy = true
            };

            return _faaService.PublishApplicationStatusSummaryAsync(message);
        }

        private FaaApplicationStatus GetApplicationStatus(ApplicationReviewStatus status)
        {
            switch (status)
            {
                case ApplicationReviewStatus.Successful:
                    return FaaApplicationStatus.Successful;
                case ApplicationReviewStatus.Unsuccessful:
                    return FaaApplicationStatus.Unsuccessful;
                default:
                    throw new NotImplementedException($"Cannot handle notification status:{status}");
            }
        }
    }
}
