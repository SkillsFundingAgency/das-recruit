using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using MediatR;
using MongoDB.Driver.Linq;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
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
            await Handle(command.ApplicationReviews, command.User, ApplicationReviewStatus.Shared);
            return Unit.Value;
        }

        public async Task<Unit> Handle(ApplicationReviewsUnsuccessfulCommand command, CancellationToken cancellationToken)
        {
            await Handle(command.ApplicationReviews, command.User, ApplicationReviewStatus.Unsuccessful, command.CandidateFeedback);
            return Unit.Value;
        }

        private async Task Handle(IEnumerable<VacancyApplication> applicationReviews, VacancyUser user, ApplicationReviewStatus status, string candidateFeedback = null)
        {
            var vacancyApplications = applicationReviews.Where(x => x.IsNotWithdrawn).ToList();
            var applicationReviewIds = vacancyApplications.Select(x => x.ApplicationReviewId);

            await _applicationReviewRepository.UpdateApplicationReviewsAsync(applicationReviewIds, user, _timeProvider.Now, status, candidateFeedback);

            var vacancy = await _vacancyRepository.GetVacancyAsync(vacancyApplications.FirstOrDefault(x => x.VacancyReference != null)!.VacancyReference.Value);
            foreach (var applicationReview in vacancyApplications.Where(x=>x.ApplicationId != null))
            {
                await _outerApiClient.Post(new PostApplicationStatusRequest(applicationReview.CandidateId,
                    applicationReview.ApplicationId!.Value, new PostApplicationStatus
                    {
                        VacancyReference = applicationReview.VacancyReference!.Value,
                        Status = status.ToString(),
                        CandidateFeedback = candidateFeedback,
                        VacancyTitle = vacancy.Title,
                        VacancyEmployerName = vacancy.EmployerName,
                        VacancyCity = vacancy.EmployerLocation.AddressLine4 ?? vacancy.EmployerLocation.AddressLine3 ?? vacancy.EmployerLocation.AddressLine2 ?? vacancy.EmployerLocation.AddressLine1 ?? "Unknown",
                        VacancyPostcode = vacancy.EmployerLocation.Postcode
                    }));
            }
        }
    }
}
