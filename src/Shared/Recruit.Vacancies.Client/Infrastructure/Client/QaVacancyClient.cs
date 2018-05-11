using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services.Models;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public class QaVacancyClient : IQaVacancyClient
    {
        private readonly IVacancyReviewRepository _reviewRepository;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IApprenticeshipProgrammeProvider _apprenticeshipProgrammesProvider;
        private readonly IMessaging _messaging;

        public QaVacancyClient(
                    IVacancyReviewRepository reviewRepository, 
                    IVacancyRepository vacancyRepository, 
                    IApprenticeshipProgrammeProvider apprenticeshipProgrammesProvider,
                    IMessaging messaging)
        {
            _reviewRepository = reviewRepository;
            _vacancyRepository = vacancyRepository;
            _apprenticeshipProgrammesProvider = apprenticeshipProgrammesProvider;
            _messaging = messaging;
        }

        public Task ApproveReferredReviewAsync(Guid reviewId, Vacancy vacancy)
        {
            return _messaging.SendCommandAsync(new ApproveReferredVacancyReviewCommand
            {
                ReviewId = reviewId,
                Vacancy = vacancy
            });
        }

        public Task ApproveReview(Guid reviewId)
        {
            return _messaging.SendCommandAsync(new ApproveVacancyReviewCommand
            {
                ReviewId = reviewId
            });
        }

        public Task<IApprenticeshipProgramme> GetApprenticeshipProgrammeAsync(string programmeId)
        {
            return _apprenticeshipProgrammesProvider.GetApprenticeshipProgrammeAsync(programmeId);
        }

        public async Task<IEnumerable<VacancyReview>> GetDashboardAsync()
        {
            // This will be replaced with a call to the query store for the dashboard view.
            var allReviews = await _reviewRepository.GetAllAsync();
            return allReviews.Where(r => (new[] { ReviewStatus.PendingReview, ReviewStatus.UnderReview }.Contains(r.Status)));
        }

        public Task<Vacancy> GetVacancyAsync(long vacancyReference)
        {
            return _vacancyRepository.GetVacancyAsync(vacancyReference);
        }

        public Task<VacancyReview> GetVacancyReviewAsync(Guid reviewId)
        {
            return _reviewRepository.GetAsync(reviewId);
        }

        public Task ReferVacancyReviewAsync(Guid reviewId)
        {
            return _messaging.SendCommandAsync(new ReferVacancyReviewCommand
            {
                ReviewId = reviewId
            });
        }

        public Task StartReview(Guid reviewId, VacancyUser user)
        {
            return _messaging.SendCommandAsync(new StartVacancyReviewCommand
            {
                ReviewId = reviewId
            });
        }
    }
}