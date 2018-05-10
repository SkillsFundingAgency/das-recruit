using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Events;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class StartVacancyReviewCommandHandler: IRequestHandler<StartVacancyReviewCommand>
    {
        private readonly IVacancyReviewRepository _vacancyReviewRepository;

        public StartVacancyReviewCommandHandler(IVacancyReviewRepository vacancyReviewRepository)
        {
            _vacancyReviewRepository = vacancyReviewRepository;
        }

        public async Task Handle(StartVacancyReviewCommand message, CancellationToken cancellationToken)
        {
            var review = await _vacancyReviewRepository.GetAsync(message.ReviewId);
            review.Status = ReviewStatus.UnderReview;
            review.ReviewedByUserId = message.UserId;

            await _vacancyReviewRepository.UpdateAsync(review);
        }
    }
}
