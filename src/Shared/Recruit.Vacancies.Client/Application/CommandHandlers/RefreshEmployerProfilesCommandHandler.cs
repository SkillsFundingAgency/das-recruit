using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class RefreshEmployerProfilesCommandHandler : IRequestHandler<RefreshEmployerProfilesCommand>
    {
        private readonly ILogger<RefreshEmployerProfilesCommandHandler> _logger;
        private readonly IEmployerProfileRepository _employerProfileRepository;
        private readonly ITimeProvider _time;

        public RefreshEmployerProfilesCommandHandler(
            ILogger<RefreshEmployerProfilesCommandHandler> logger,
            IEmployerProfileRepository employerProfileRepository,
            ITimeProvider time)
        {
            _logger = logger;
            _employerProfileRepository = employerProfileRepository;
            _time = time;
        }

        public async Task Handle(RefreshEmployerProfilesCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Refreshing Employer Profiles for {employerAccountId}", message.EmployerAccountId);

            var tasks = new List<Task>();

            // Get all current profiles for the employer
            var profiles = await _employerProfileRepository.GetEmployerProfilesForEmployerAsync(message.EmployerAccountId);

            foreach (var legalEntity in message.LegalEntityIds)
            {
                if (!profiles.Any(x => x.LegalEntityId == legalEntity))
                {
                    var currentTime = _time.Now;

                    // Create new profile
                    var newProfile = new EmployerProfile
                    {
                        Id = Guid.NewGuid(),
                        EmployerAccountId = message.EmployerAccountId,
                        LegalEntityId = legalEntity,
                        CreatedDate = currentTime
                    };

                    _logger.LogInformation("Adding new profile for employer account: {employerAccountId} and legal entity id: {legalEntityId}", message.EmployerAccountId, legalEntity);
                    tasks.Add(_employerProfileRepository.CreateAsync(newProfile));
                }
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation($"Added {tasks.Count} new profile/s for {{employerAccountId}}", message.EmployerAccountId);
        }
    }
}
