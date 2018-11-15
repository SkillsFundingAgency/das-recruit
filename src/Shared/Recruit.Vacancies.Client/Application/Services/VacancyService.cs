using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Engine;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public class VacancyService : IVacancyService
    {
        private readonly ILogger<VacancyService> _logger;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly IMessaging _messaging;
        private readonly RuleSet<Vacancy> _vacancyRuleSet;
        private readonly IVacancyReviewRepository _vacancyReviewRepository;

        public VacancyService(
            ILogger<VacancyService> logger, IVacancyRepository vacancyRepository, 
            ITimeProvider timeProvider, IMessaging messaging,
            RuleSet<Vacancy> vacancyRuleSet, IVacancyReviewRepository vacancyReviewRepository)
        {
            _logger = logger;
            _vacancyRepository = vacancyRepository;
            _timeProvider = timeProvider;
            _messaging = messaging;
            _vacancyRuleSet = vacancyRuleSet;
            _vacancyReviewRepository = vacancyReviewRepository;
        }

        public async Task CloseExpiredVacancy(Guid vacancyId)
        {
            _logger.LogInformation("Closing vacancy {vacancyId}.", vacancyId);

            var vacancy = await _vacancyRepository.GetVacancyAsync(vacancyId);

            await CloseVacancyAsync(vacancy);
        }

        private async Task CloseVacancyAsync(Vacancy vacancy)
        {
            vacancy.ClosedDate = _timeProvider.Now;
            vacancy.Status = VacancyStatus.Closed;

            await _vacancyRepository.UpdateAsync(vacancy);

            await _messaging.PublishEvent(new VacancyClosedEvent
            {
                EmployerAccountId = vacancy.EmployerAccountId,
                VacancyReference = vacancy.VacancyReference.Value,
                VacancyId = vacancy.Id
            });
        }

        public async Task CloseVacancyImmediately(Guid vacancyId, VacancyUser user)
        {
            _logger.LogInformation("Closing vacancy {vacancyId} by user {userEmail}.", vacancyId, user.Email);

            var vacancy = await _vacancyRepository.GetVacancyAsync(vacancyId);

            vacancy.ClosedByUser = user;

            await CloseVacancyAsync(vacancy);
        }

        public async Task PerformRulesCheckAsync(Guid reviewId)
        {
            var review = await _vacancyReviewRepository.GetAsync(reviewId);
            var outcome = await _vacancyRuleSet.EvaluateAsync(review.VacancySnapshot);
            review.AutomatedQaOutcome = outcome;
            review.Status = ReviewStatus.PendingReview;
            review.AutomatedQaOutcomeIndicators =
                outcome
                    .RuleOutcomes
                    .SelectMany(o => o.Details)
                    .Where(s => s.Score > 0) //Eventually this should be check against threshold for that rule
                    .Select(r => new RuleOutcomeIndicator {RuleOutcomeId = r.Id, IsReferred = true})
                    .ToList();
            await _vacancyReviewRepository.UpdateAsync(review);
        }
    }
}