using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Encoding;
using SFA.DAS.ProviderRelationships.Messages.Events;
using SFA.DAS.ProviderRelationships.Types.Models;

namespace Esfa.Recruit.Vacancies.Jobs.ExternalSystemEventHandlers
{
    public class UpdatedPermissionsExternalSystemEventsHandler : IHandleMessages<UpdatedPermissionsEvent>
    {
        private readonly ILogger<UpdatedPermissionsExternalSystemEventsHandler> _logger;
        private readonly IRecruitQueueService _recruitQueueService;
        private readonly IEmployerAccountProvider _employerAccountProvider;
        private readonly IEncodingService _encoder;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly IVacancyQuery _vacancyQuery;
        private readonly IMessaging _messaging;
        private string ExternalSystemEventHandlerName => GetType().Name;

        public UpdatedPermissionsExternalSystemEventsHandler(ILogger<UpdatedPermissionsExternalSystemEventsHandler> logger, RecruitWebJobsSystemConfiguration jobsConfig,
                                                IRecruitQueueService recruitQueueService,
                                                IEmployerAccountProvider employerAccountProvider, IEncodingService encoder, IVacancyQuery vacancyQuery,
                                                IMessaging messaging)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _recruitQueueService = recruitQueueService;
            _employerAccountProvider = employerAccountProvider;
            _encoder = encoder;
            _vacancyQuery = vacancyQuery;
            _messaging = messaging;
        }

        public async Task Handle(UpdatedPermissionsEvent message, IMessageHandlerContext context)
        {
            if (_jobsConfig.DisabledJobs.Contains(ExternalSystemEventHandlerName))
            {
                _logger.LogDebug($"{ExternalSystemEventHandlerName} is disabled, skipping ...");
                return;
            }

            _logger.LogInformation($"Attempting to process {nameof(UpdatedPermissionsEvent)} : {{@eventMessage}}", message);

            if (message.UserRef.HasValue == false)
            {
                _logger.LogInformation($"Not handling Provider {nameof(Operation.Recruitment)} Permission being revoked as it is a consequence of Provider being blocked by QA on Recruit.");
                return;
            }

            if (message.GrantedOperations.Contains(Operation.Recruitment) == false)
            {
                var legalEntity = await GetAssociatedLegalEntity(message);

                if (legalEntity == null)
                {
                    throw new Exception($"Could not find matching Account Legal Entity Id {message.AccountLegalEntityId} for Employer Account {message.AccountId}");
                }

                var noOfAssociatedVacancies = (await _vacancyQuery.GetNoOfProviderOwnedVacanciesForLegalEntityAsync(message.Ukprn, legalEntity.LegalEntityId));

                if (noOfAssociatedVacancies > 0)
                {
                    await _recruitQueueService.AddMessageAsync(new TransferVacanciesFromProviderQueueMessage
                    {
                        Ukprn = message.Ukprn,
                        LegalEntityId = legalEntity.LegalEntityId,
                        UserRef = message.UserRef.Value,
                        UserEmailAddress = message.UserEmailAddress,
                        UserName = $"{message.UserFirstName} {message.UserLastName}",
                        TransferReason = TransferReason.EmployerRevokedPermission
                    });

                    await _messaging.SendCommandAsync(new SetupProviderCommand(message.Ukprn));
                }
            }
        }

        private async Task<LegalEntity> GetAssociatedLegalEntity(UpdatedPermissionsEvent message)
        {
            var employerAccountId = _encoder.Encode(message.AccountId, EncodingType.AccountId);
            var legalEntities = await _employerAccountProvider.GetLegalEntitiesConnectedToAccountAsync(employerAccountId);
            var legalEntity = legalEntities.FirstOrDefault(le => le.AccountLegalEntityId == message.AccountLegalEntityId);
            return legalEntity == null ? null : LegalEntityMapper.MapFromAccountApiLegalEntity(legalEntity);
        }
    }
}