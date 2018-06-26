using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class ApplicationReviewCommandHandler : IRequestHandler<ApplicationReviewSuccessfulCommand>
    {
        private readonly ILogger<ApplicationReviewCommandHandler> _logger;
        private readonly IApplicationReviewRepository _applicationReviewRepository;
        private readonly ITimeProvider _timeProvider;

        public ApplicationReviewCommandHandler(
            ILogger<ApplicationReviewCommandHandler> logger, 
            IApplicationReviewRepository applicationReviewRepository,
            ITimeProvider timeProvider)
        {
            _logger = logger;
            _applicationReviewRepository = applicationReviewRepository;
            _timeProvider = timeProvider;
        }

        public async Task Handle(ApplicationReviewSuccessfulCommand message, CancellationToken cancellationToken)
        {
            var applicationReview = await _applicationReviewRepository.GetAsync(message.ApplicationReviewId);

            applicationReview.Status = ApplicationReviewStatus.Successful;
            applicationReview.StatusUpdatedDate = _timeProvider.Now;
            applicationReview.StatusUpdatedBy = message.User;

            await _applicationReviewRepository.UpdateAsync(applicationReview);
        }
    }
}
