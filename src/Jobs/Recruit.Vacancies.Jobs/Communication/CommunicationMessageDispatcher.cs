using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using SFA.DAS.Notifications.Messages.Commands;

namespace Esfa.Recruit.Vacancies.Jobs.Communication
{
    public sealed class CommunicationMessageDispatcher
    {
        private const string UserNameTokenKey = "user-name";

        private readonly ILogger<CommunicationMessageDispatcher> _logger;
        private readonly ICommunicationRepository _repository;
        private readonly INotificationService _notificationService;
        private readonly ITimeProvider _timeProvider;
        private readonly RetryPolicy _retryPolicy;

        public CommunicationMessageDispatcher(ILogger<CommunicationMessageDispatcher> logger, ICommunicationRepository repository,
                                            INotificationService notificationService, ITimeProvider timeProvider)
        {
            _logger = logger;
            _repository = repository;
            _notificationService = notificationService;
            _timeProvider = timeProvider;
            _retryPolicy = GetRetryPolicy();
        }

        public async Task Run(CommunicationMessageIdentifier commMsgId)
        {
            _logger.LogInformation($"Retrieving {nameof(CommunicationMessage)}:{commMsgId.Id.ToString()} from CommunicationMessages Store");
            var commMsg = await _repository.GetAsync(commMsgId.Id);

            if (commMsg == null)
            {
                throw new ArgumentException($"Could not find communication message: {commMsgId.Id} in CommunicationMessages Store.");
            }

            try
            {
                _logger.LogInformation($"Retrieving {nameof(CommunicationMessage)}:{commMsgId.Id.ToString()} from CommunicationMessages Store");

                switch (commMsg.Channel)
                {
                    case DeliveryChannel.Sms:
                        throw new NotSupportedException("Currently not supporting sending SMS via DAS Notifications.");
                    case DeliveryChannel.Email:
                    case DeliveryChannel.Default:
                        await SendEmail(commMsg);
                    break;
                }

                commMsg.Status = CommunicationMessageStatus.Sent;
                commMsg.DispatchOutcome = DispatchOutcome.Succeeded;
                commMsg.DispatchDateTime = _timeProvider.Now;
                await _repository.UpdateAsync(commMsg);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Failed to submit communication message {commMsg.Id} to DAS Notifications API.");
                commMsg.Status = CommunicationMessageStatus.NotSent;
                commMsg.DispatchDateTime = _timeProvider.Now;
                commMsg.DispatchOutcome = DispatchOutcome.Failed;
                await _repository.UpdateAsync(commMsg);
            }
        }

        public async Task SendEmail(CommunicationMessage request)
        {
            _logger.LogInformation($"Trying to send message of type {request.RequestType} to {request.Recipient.UserId}");

            var readOnlyDictionary = request.DataItems.ToDictionary(x => x.Key, x => x.Value);
            readOnlyDictionary.Add(UserNameTokenKey, request.Recipient.Name);
            
            var email = new SendEmailCommand(request.TemplateId, request.Recipient.Email, readOnlyDictionary);

            await _retryPolicy.Execute(context => _notificationService.Send(email),
                                            new Context(nameof(SendEmail)));
            _logger.LogInformation($"Successfully sent message of type {request.RequestType} to {request.Recipient.UserId}");
        }

        private RetryPolicy GetRetryPolicy()
        {
            return Policy
                    .Handle<HttpRequestException>()
                    .WaitAndRetry(new[]
                    {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(4)
                    }, (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Error calling DAS Notification API for - {context.OperationKey} Reason: {exception.Message}. Retrying in {timeSpan.Seconds} secs...attempt: {retryCount}");
                    });
        }
    }
}