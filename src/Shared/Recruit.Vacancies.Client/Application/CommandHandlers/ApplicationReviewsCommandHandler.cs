using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using MediatR;
using SFA.DAS.Encoding;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;

public class ApplicationReviewPendingUnsuccessfulFeedbackCommandHandler(
    IVacancyRepository vacancyRepository,
    IApplicationReviewRepositoryRunner applicationReviewRepositoryRunner)
    : IRequestHandler<ApplicationReviewPendingUnsuccessfulFeedbackCommand, Unit>
{
    public async Task<Unit> Handle(ApplicationReviewPendingUnsuccessfulFeedbackCommand request, CancellationToken cancellationToken)
    {
        var vacancy = await vacancyRepository.GetVacancyAsync(request.VacancyId);
        await applicationReviewRepositoryRunner.UpdateApplicationReviewsPendingUnsuccessfulFeedback(vacancy.VacancyReference!.Value!,request.User, DateTime.UtcNow, request.Feedback);
        return Unit.Value;
    }
}

public class ApplicationReviewsCommandHandler(
    IApplicationReviewRepository applicationReviewRepository,
    ITimeProvider timeProvider,
    IOuterApiClient outerApiClient,
    IVacancyRepository vacancyRepository,
    IApplicationReviewRepositoryRunner applicationReviewRepositoryRunner,
    IEncodingService encodingService)
    :
        IRequestHandler<ApplicationReviewsSharedCommand, Unit>,
        IRequestHandler<ApplicationReviewsUnsuccessfulCommand, Unit>
{
    public async Task<Unit> Handle(ApplicationReviewsSharedCommand command, CancellationToken cancellationToken)
    {
        await Handle(command.ApplicationReviews.ToList(), command.User, command.Status, command.VacancyId, command.TemporaryStatus, vacancyReference: command.VacancyReference);
        return Unit.Value;
    }

    public async Task<Unit> Handle(ApplicationReviewsUnsuccessfulCommand command, CancellationToken cancellationToken)
    {
        await Handle(command.ApplicationReviews.ToList(), command.User, command.Status, command.VacancyId,command.TemporaryStatus, command.CandidateFeedback);
        return Unit.Value;
    }

    private async Task Handle(List<Guid> applicationReviews,
        VacancyUser user,
        ApplicationReviewStatus? status,
        Guid vacancyId,
        ApplicationReviewStatus? temporaryStatus,
        string candidateFeedback = null,
        long? vacancyReference = null)
    {
        await applicationReviewRepositoryRunner.UpdateApplicationReviewsAsync(applicationReviews, user, timeProvider.Now, status, temporaryStatus, candidateFeedback, vacancyReference);

        if (status is not (ApplicationReviewStatus.Unsuccessful or ApplicationReviewStatus.Shared))
        {
            return; // nothing to do for other statuses
        }

        var vacancy = await vacancyRepository.GetVacancyAsync(vacancyId);
        var applicationReviewsByReference =
            await applicationReviewRepository.GetForVacancyAsync<ApplicationReview>(vacancy.VacancyReference!.Value);

        var eligibleReviews = applicationReviewsByReference
            .Where(x => !x.IsWithdrawn
                        && x.Application?.IsFaaV2Application == true
                        && applicationReviews.Contains(x.Id));

        switch (status)
        {
            case ApplicationReviewStatus.Unsuccessful:
                {
                    await SendUnSuccessfulNotification(eligibleReviews.ToList(), vacancy, status, candidateFeedback);
                    return;
                }
            case ApplicationReviewStatus.Shared:
                {
                    await SendSharedNotification(eligibleReviews.ToList(), vacancy);
                    return;
                }
            default:
                return;
        }
    }

    private async Task SendSharedNotification(List<ApplicationReview> reviews, Vacancy vacancy)
    {
        var tasks = reviews.Select(applicationReview =>
        {
            long accountId = encodingService.Decode(vacancy.EmployerAccountId, EncodingType.AccountId);
            var requestData = new PostApplicationSharedNotificationApiRequest.PostApplicationSharedNotificationApiRequestData
            {
                AdvertTitle = vacancy.Title,
                ApplicationId = applicationReview.Application!.ApplicationId,
                HashAccountId = vacancy.EmployerAccountId,
                AccountId = accountId, 
                TrainingProvider = vacancy.TrainingProvider.Name,
                VacancyId = vacancy.Id,
                VacancyReference = vacancy.VacancyReference!.Value
            };

            var request = new PostApplicationSharedNotificationApiRequest(requestData);

            return outerApiClient.Post(request);
        });

        await Task.WhenAll(tasks);
    }

    private async Task SendUnSuccessfulNotification(List<ApplicationReview> reviews, Vacancy vacancy, ApplicationReviewStatus? status, string candidateFeedback)
    {
        var tasks = reviews.Select(applicationReview =>
        {
            var request = new PostApplicationStatusRequest(applicationReview.CandidateId,
                applicationReview.Application.ApplicationId, new PostApplicationStatus
                {
                    VacancyReference = applicationReview.VacancyReference,
                    Status = status.ToString(),
                    CandidateFeedback = candidateFeedback,
                    VacancyTitle = vacancy.Title,
                    VacancyEmployerName = vacancy.EmployerName,
                    VacancyLocation = vacancy.GetVacancyLocation()
                });

            return outerApiClient.Post(request);
        });

        await Task.WhenAll(tasks);
    }
}