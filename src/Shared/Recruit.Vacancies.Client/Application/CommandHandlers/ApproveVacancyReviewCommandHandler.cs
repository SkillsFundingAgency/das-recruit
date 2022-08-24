using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class ApproveVacancyReviewCommandHandler: IRequestHandler<ApproveVacancyReviewCommand, Unit>
    {
        private readonly ILogger<ApproveVacancyReviewCommandHandler> _logger;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IVacancyReviewRepository _vacancyReviewRepository;
        private readonly IMessaging _messaging;
        private readonly AbstractValidator<VacancyReview> _vacancyReviewValidator;
        private readonly ITimeProvider _timeProvider;
        private readonly IBlockedOrganisationQuery _blockedOrganisationQuery;
        private readonly IEmployerDashboardProjectionService _dashboardService;
        private readonly ICommunicationQueueService _communicationQueueService;

        public ApproveVacancyReviewCommandHandler(ILogger<ApproveVacancyReviewCommandHandler> logger,
                                        IVacancyReviewRepository vacancyReviewRepository,
                                        IVacancyRepository vacancyRepository,
                                        IMessaging messaging,
                                        AbstractValidator<VacancyReview> vacancyReviewValidator,
                                        ITimeProvider timeProvider,
                                        IBlockedOrganisationQuery blockedOrganisationQuery, 
                                        IEmployerDashboardProjectionService dashboardService,
                                        ICommunicationQueueService communicationQueueService)
        {
            _logger = logger;
            _vacancyRepository = vacancyRepository;
            _vacancyReviewRepository = vacancyReviewRepository;
            _messaging = messaging;
            _vacancyReviewValidator = vacancyReviewValidator;
            _timeProvider = timeProvider;
            _blockedOrganisationQuery = blockedOrganisationQuery;
            _dashboardService = dashboardService;
            _communicationQueueService = communicationQueueService;
        }

        public async Task<Unit> Handle(ApproveVacancyReviewCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Approving review {reviewId}.", message.ReviewId);

            var review = await _vacancyReviewRepository.GetAsync(message.ReviewId);
            var vacancy = await _vacancyRepository.GetVacancyAsync(review.VacancyReference);

            if (!review.CanApprove)
            {
                _logger.LogWarning($"Unable to approve review {{reviewId}} due to review having a status of {review.Status}.", message.ReviewId);
                return Unit.Value;
            }

            review.ManualOutcome = ManualQaOutcome.Approved;
            review.Status = ReviewStatus.Closed;
            review.ClosedDate = _timeProvider.Now;
            review.ManualQaComment = message.ManualQaComment;
            review.ManualQaFieldIndicators = message.ManualQaFieldIndicators;
            review.ManualQaFieldEditIndicators = message.ManualQaFieldEditIndicators;
            foreach (var automatedQaOutcomeIndicator in review.AutomatedQaOutcomeIndicators)
            {
                automatedQaOutcomeIndicator.IsReferred = message.SelectedAutomatedQaRuleOutcomeIds
                    .Contains(automatedQaOutcomeIndicator.RuleOutcomeId);
            }

            Validate(review);

            await _vacancyReviewRepository.UpdateAsync(review);

            var closureReason = await TryGetReasonToCloseVacancy(review, vacancy);

            if (closureReason != null)
            {
                await CloseVacancyAsync(vacancy, closureReason.Value);
                await SendNotificationToEmployerAsync(vacancy.TrainingProvider.Ukprn.GetValueOrDefault(), vacancy.EmployerAccountId);
                await _dashboardService.ReBuildDashboardAsync(vacancy.EmployerAccountId);
                return Unit.Value;
            }

            await PublishVacancyReviewApprovedEventAsync(message, review);    
            return Unit.Value;
        }

        private Task SendNotificationToEmployerAsync(long ukprn, string employerAccountId)
        {
            var communicationRequest = CommunicationRequestFactory.GetProviderBlockedEmployerNotificationForLiveVacanciesRequest(ukprn, employerAccountId);

            return _communicationQueueService.AddMessageAsync(communicationRequest);
        }

        private async Task<ClosureReason?> TryGetReasonToCloseVacancy(VacancyReview review, Vacancy vacancy)
        {
            if (HasVacancyBeenTransferredSinceReviewWasCreated(review, vacancy))
            {
                return vacancy.TransferInfo.Reason == TransferReason.EmployerRevokedPermission ? 
                    ClosureReason.TransferredByEmployer : ClosureReason.TransferredByQa;
            }

            if(await HasProviderBeenBlockedSinceReviewWasCreatedAsync(vacancy))
                return ClosureReason.BlockedByQa;

            return null;
        }

        private void Validate(VacancyReview review)
        {
            var validationResult = _vacancyReviewValidator.Validate(review);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }

        private bool HasVacancyBeenTransferredSinceReviewWasCreated(VacancyReview review, Vacancy vacancy)
        {
            return review.VacancySnapshot.TransferInfo == null && vacancy.TransferInfo != null;
        }

        private async Task<bool> HasProviderBeenBlockedSinceReviewWasCreatedAsync(Vacancy vacancy)
        {
            var blockedProvider = await _blockedOrganisationQuery.GetByOrganisationIdAsync(vacancy.TrainingProvider.Ukprn.ToString());
            return blockedProvider?.BlockedStatus == BlockedStatus.Blocked;
        }

        private Task CloseVacancyAsync(Vacancy vacancy, ClosureReason closureReason)
        {
            vacancy.Status = VacancyStatus.Closed;
            vacancy.ClosedDate = _timeProvider.Now;
            vacancy.ClosedByUser = vacancy.TransferInfo?.TransferredByUser;
            vacancy.ClosureReason = closureReason;
            return _vacancyRepository.UpdateAsync(vacancy);
        }

        private Task PublishVacancyReviewApprovedEventAsync(ApproveVacancyReviewCommand message, VacancyReview review)
        {
            return _messaging.PublishEvent(new VacancyReviewApprovedEvent
            {
                ReviewId = message.ReviewId,
                VacancyReference = review.VacancyReference
            });
        }
    }
}