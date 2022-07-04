using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
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
        private readonly IEncodingService _encoder;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly IMessaging _messaging;
        
        private string ExternalSystemEventHandlerName => GetType().Name;

        public UpdatedPermissionsExternalSystemEventsHandler(ILogger<UpdatedPermissionsExternalSystemEventsHandler> logger, RecruitWebJobsSystemConfiguration jobsConfig,
                                                IRecruitQueueService recruitQueueService, IEncodingService encoder,
                                                IMessaging messaging)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _recruitQueueService = recruitQueueService;
            _encoder = encoder;
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
            
            var employerAccountId = _encoder.Encode(message.AccountId, EncodingType.AccountId);
            var employerAccountLegalEntityId = _encoder.Encode(message.AccountLegalEntityId, EncodingType.PublicAccountLegalEntityId);

            if (message.GrantedOperations.Contains(Operation.Recruitment) == false)
            {
                _logger.LogInformation($"Transferring vacancies from Provider {message.Ukprn} to Employer {message.AccountId}");

                await _recruitQueueService.AddMessageAsync(new TransferVacanciesFromProviderQueueMessage
                {
                    Ukprn = message.Ukprn,
                    EmployerAccountId = employerAccountId,
                    AccountLegalEntityPublicHashedId = employerAccountLegalEntityId,
                    UserRef = message.UserRef.Value,
                    UserEmailAddress = message.UserEmailAddress,
                    UserName = $"{message.UserFirstName} {message.UserLastName}",
                    TransferReason = TransferReason.EmployerRevokedPermission
                });
            }
            else if (message.GrantedOperations.Contains(Operation.RecruitmentRequiresReview) == false)
            {
                _logger.LogInformation($"Transferring vacancies from Employer Review to QA Review for Provider {message.Ukprn}");

                await _recruitQueueService.AddMessageAsync(new TransferVacanciesFromEmployerReviewToQAReviewQueueMessage
                {
                    Ukprn = message.Ukprn,
                    AccountLegalEntityPublicHashedId = employerAccountLegalEntityId,
                    UserRef = message.UserRef.Value,
                    UserEmailAddress = message.UserEmailAddress,
                    UserName = $"{message.UserFirstName} {message.UserLastName}"
                });
            }

            await _messaging.SendCommandAsync(new SetupProviderCommand(message.Ukprn));
        }
    }
}