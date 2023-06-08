using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
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
            await Handle(command.ApplicationReviewIds, command.User, ApplicationReviewStatus.Shared);
            return Unit.Value;
        }

        private async Task Handle(IEnumerable<Guid> applicationReviewIds, VacancyUser user, ApplicationReviewStatus status)
        {
            await _applicationReviewRepository.UpdateApplicationReviewsAsync(applicationReviewIds, user, _timeProvider.Now, status);
        }
    }
}
