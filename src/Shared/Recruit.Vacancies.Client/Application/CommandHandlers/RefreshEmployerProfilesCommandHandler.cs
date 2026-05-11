using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerProfile;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class RefreshEmployerProfilesCommandHandler(
        ILogger<RefreshEmployerProfilesCommandHandler> logger,
        IEmployerProfileService employerProfileService,
        ITimeProvider time,
        IEmployerVacancyClient employerVacancyClient)
        : IRequestHandler<RefreshEmployerProfilesCommand, Unit>
    {
        public async Task<Unit> Handle(RefreshEmployerProfilesCommand message, CancellationToken cancellationToken)
        {
            logger.LogInformation("Refreshing Employer Profiles for {employerAccountId}", message.EmployerAccountId);
            var employerVacancyInfoTask =
                employerVacancyClient.GetEditVacancyInfoAsync(message.EmployerAccountId);
            var tasks = new List<Task>();
            var editVacancyInfo = employerVacancyInfoTask.Result;
            // Get all current profiles for the employer
            var profiles = await employerProfileService.GetEmployerProfilesForEmployerAsync(message.EmployerAccountId);

            foreach (var accountLegalEntityPublicHashedId in message.AccountLegalEntityPublicHashedIds)
            {
                try
                {
                    var selectedOrganisation = editVacancyInfo.LegalEntities.Single(l => l.AccountLegalEntityPublicHashedId == accountLegalEntityPublicHashedId);

                    if (profiles.All(x => x.AccountLegalEntityPublicHashedId != accountLegalEntityPublicHashedId))
                    { 
                        // Create new profile
                        var newProfile = new EmployerProfile
                        {
                            EmployerAccountId = message.EmployerAccountId,
                            AccountLegalEntityPublicHashedId = selectedOrganisation.AccountLegalEntityPublicHashedId
                        };

                        logger.LogInformation("Adding new profile for employer account: {employerAccountId} and Account LegalEntityPublicHashed id: {accountLegalEntityPublicHashedId}", message.EmployerAccountId, accountLegalEntityPublicHashedId);
                        tasks.Add(employerProfileService.CreateAsync(newProfile));
                    }

                }
                catch (Exception)
                {
                    logger.LogError("Error while processing employer account: {employerAccountId} and Account LegalEntityPublicHashed id: {accountLegalEntityPublicHashedId}", message.EmployerAccountId, accountLegalEntityPublicHashedId);
                    throw;
                }


            }
            await Task.WhenAll(tasks);
            logger.LogInformation("Added {Count} new profile/s for {employerAccountId}", tasks.Count, message.EmployerAccountId);
            return Unit.Value;
        }
    }
}
