using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using MediatR;
namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class ApplicationReviewsToUnsuccessfulCommandHandler :
        IRequestHandler<ApplicationReviewsToUnsuccessfulCommand, Unit>
    {
        private readonly IApplicationReviewRepository _applicationReviewRepository;
        private readonly ITimeProvider _timeProvider;

        public ApplicationReviewsToUnsuccessfulCommandHandler(
            IApplicationReviewRepository applicationReviewRepository,
            ITimeProvider timeProvider)
        {
            _applicationReviewRepository = applicationReviewRepository;
            _timeProvider = timeProvider;
        }

        public async Task<Unit> Handle(ApplicationReviewsToUnsuccessfulCommand command, CancellationToken cancellationToken)
        {
            await Handle(command.ApplicationReviews, command.User, ApplicationReviewStatus.Unsuccessful, command.CandidateFeedback);
            return Unit.Value;
        }

        private async Task Handle(IEnumerable<VacancyApplication> applicationReviews, VacancyUser user, ApplicationReviewStatus status,string candidateFeedback)
        {
            var applicationReviewIds = applicationReviews.Where(x => x.IsNotWithdrawn).Select(x => x.ApplicationReviewId);

            await _applicationReviewRepository.UpdateApplicationReviewsAsync(applicationReviewIds, user, _timeProvider.Now, status, candidateFeedback);
        }
    }
}
