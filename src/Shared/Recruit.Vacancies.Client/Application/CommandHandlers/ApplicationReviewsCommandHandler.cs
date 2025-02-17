using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;

public class ApplicationReviewPendingUnsuccessfulFeedbackCommandHandler(IVacancyRepository vacancyRepository, IApplicationReviewRepository applicationReviewRepository) : IRequestHandler<ApplicationReviewPendingUnsuccessfulFeedbackCommand, Unit>
{
    public async Task<Unit> Handle(ApplicationReviewPendingUnsuccessfulFeedbackCommand request, CancellationToken cancellationToken)
    {
        var vacancy = await vacancyRepository.GetVacancyAsync(request.VacancyId);
        
        await applicationReviewRepository.UpdateApplicationReviewsPendingUnsuccessfulFeedback(vacancy.VacancyReference!.Value!,request.User, DateTime.UtcNow, request.Feedback);
        return Unit.Value;
    }
}

public class ApplicationReviewsCommandHandler :
    IRequestHandler<ApplicationReviewsSharedCommand, Unit>,
    IRequestHandler<ApplicationReviewsUnsuccessfulCommand, Unit>
{
    private readonly IApplicationReviewRepository _applicationReviewRepository;
    private readonly ITimeProvider _timeProvider;
    private readonly IOuterApiClient _outerApiClient;
    private readonly IVacancyRepository _vacancyRepository;

    public ApplicationReviewsCommandHandler(
        IApplicationReviewRepository applicationReviewRepository,
        ITimeProvider timeProvider, 
        IOuterApiClient outerApiClient,
        IVacancyRepository vacancyRepository)
    {
        _applicationReviewRepository = applicationReviewRepository;
        _timeProvider = timeProvider;
        _outerApiClient = outerApiClient;
        _vacancyRepository = vacancyRepository;
    }

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

    private async Task Handle(List<Guid> applicationReviews, VacancyUser user, ApplicationReviewStatus? status, Guid vacancyId, ApplicationReviewStatus? temporaryStatus, string candidateFeedback = null, long? vacancyReference = null)
    {
        await _applicationReviewRepository.UpdateApplicationReviewsAsync(applicationReviews, user, _timeProvider.Now, status, temporaryStatus, candidateFeedback, vacancyReference);

        if (status == ApplicationReviewStatus.Unsuccessful)
        {
            var vacancy = await _vacancyRepository.GetVacancyAsync(vacancyId);
            var applicationReviewsByReference =
                await _applicationReviewRepository.GetForVacancyAsync<ApplicationReview>(vacancy.VacancyReference!.Value);
                
            foreach (var applicationReview in applicationReviewsByReference.Where(x => x.Application!.IsFaaV2Application && !x.IsWithdrawn))
            {
                await _outerApiClient.Post(new PostApplicationStatusRequest(applicationReview.CandidateId,
                    applicationReview.Application.ApplicationId, new PostApplicationStatus
                    {
                        VacancyReference = applicationReview.VacancyReference,
                        Status = status.ToString(),
                        CandidateFeedback = candidateFeedback,
                        VacancyTitle = vacancy.Title,
                        VacancyEmployerName = vacancy.EmployerName,
                        VacancyCity = vacancy.EmployerLocation.AddressLine4 ??
                                      vacancy.EmployerLocation.AddressLine3 ??
                                      vacancy.EmployerLocation.AddressLine2 ??
                                      vacancy.EmployerLocation.AddressLine1 ?? "Unknown",
                        VacancyPostcode = vacancy.EmployerLocation.Postcode
                    }));
            }
        }
    }
}