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
    public class ApplicationReviewsCommandHandler :
        IRequestHandler<ApplicationReviewsSharedCommand, Unit>
    {
        private readonly IApplicationReviewRepository _applicationReviewRepository;
        private readonly ITimeProvider _timeProvider;

        public ApplicationReviewsCommandHandler(
            IApplicationReviewRepository applicationReviewRepository,
            ITimeProvider timeProvider)
        {
            _applicationReviewRepository = applicationReviewRepository;
            _timeProvider = timeProvider;
        }

        public async Task<Unit> Handle(ApplicationReviewsSharedCommand command, CancellationToken cancellationToken)
        {
            await Handle(command.ApplicationReviews, command.User, ApplicationReviewStatus.Shared);
            return Unit.Value;
        }

        private async Task Handle(IEnumerable<VacancyApplication> applicationReviews, VacancyUser user, ApplicationReviewStatus status)
        {
            var applicationReviewIds = applicationReviews.Where(x => x.IsNotWithdrawn).Select(x => x.ApplicationReviewId);

            await _applicationReviewRepository.UpdateApplicationReviewsAsync(applicationReviewIds, user, _timeProvider.Now, status);
        }
    }
}
