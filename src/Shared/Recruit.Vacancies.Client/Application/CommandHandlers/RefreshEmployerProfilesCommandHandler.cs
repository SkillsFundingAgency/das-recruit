using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class RefreshEmployerProfilesCommandHandler : IRequestHandler<RefreshEmployerProfilesCommand>
    {
        private readonly ILogger<RefreshEmployerProfilesCommandHandler> _logger;
        private readonly IEmployerProfileRepository _employerProfileRepository;
        private readonly ITimeProvider _time;
        private readonly IEmployerVacancyClient _employerVacancyClient;

        public RefreshEmployerProfilesCommandHandler(
            ILogger<RefreshEmployerProfilesCommandHandler> logger,
            IEmployerProfileRepository employerProfileRepository,
            ITimeProvider time, IEmployerVacancyClient employerVacancyClient)
        {
            _logger = logger;
            _employerProfileRepository = employerProfileRepository;
            _time = time;
            _employerVacancyClient = employerVacancyClient;
        }

        public async Task Handle(RefreshEmployerProfilesCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Refreshing Employer Profiles for {employerAccountId}", message.EmployerAccountId);
            var employerVacancyInfoTask =
                _employerVacancyClient.GetEditVacancyInfoAsync(message.EmployerAccountId);
            var tasks = new List<Task>();
            var editVacancyInfo = employerVacancyInfoTask.Result;
            // Get all current profiles for the employer
            var profiles = await _employerProfileRepository.GetEmployerProfilesForEmployerAsync(message.EmployerAccountId);

            foreach (var accountLegalEntityPublicHashedId in message.AccountLegalEntityPublicHashedIds)
            {
                var selectedOrganisation =
                    editVacancyInfo.LegalEntities.Single(l => l.AccountLegalEntityPublicHashedId == accountLegalEntityPublicHashedId);
                if (!profiles.Any(x => x.AccountLegalEntityPublicHashedId == accountLegalEntityPublicHashedId))
                {
                    var currentTime = _time.Now;

                    // Create new profile
                    var newProfile = new EmployerProfile
                    {
                        EmployerAccountId = message.EmployerAccountId,
                        CreatedDate = currentTime,
                        AccountLegalEntityPublicHashedId = selectedOrganisation.AccountLegalEntityPublicHashedId
                    };

                    _logger.LogInformation("Adding new profile for employer account: {employerAccountId} and Account LegalEntityPublicHashed id: {accountLegalEntityPublicHashedId}", message.EmployerAccountId, accountLegalEntityPublicHashedId);
                    tasks.Add(_employerProfileRepository.CreateAsync(newProfile));
                }
            }
            await Task.WhenAll(tasks);
            _logger.LogInformation($"Added {tasks.Count} new profile/s for {{employerAccountId}}", message.EmployerAccountId);
        }
    }
}
