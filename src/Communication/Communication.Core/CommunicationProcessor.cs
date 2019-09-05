using System.Collections.Generic;
using System.Threading.Tasks;
using Communication.Types.Exceptions;
using Communication.Types;
using Communication.Types.Extensions;
using Communication.Types.Interfaces;
using System.Linq;
using System;
using Microsoft.Extensions.Logging;

namespace Communication.Core
{
    public class CommunicationProcessor : ICommunicationProcessor
    {
        private readonly IDictionary<string, IParticipantResolver> _participantResolvers = new Dictionary<string, IParticipantResolver>();
        private readonly IDictionary<string, IUserPreferencesProvider> _userPreferencesProviders = new Dictionary<string, IUserPreferencesProvider>();
        private readonly IDictionary<string, IEntityDataItemProvider> _entityDataItemProviders = new Dictionary<string, IEntityDataItemProvider>();
        private readonly IDictionary<string, ITemplateIdProvider> _templateIdProviders = new Dictionary<string, ITemplateIdProvider>();
        private readonly IDictionary<string, ICompositeDataItemProvider> _compositeDataItemProviders = new Dictionary<string, ICompositeDataItemProvider>();
        private readonly ILogger<CommunicationProcessor> _logger;

        public CommunicationProcessor(
            IEnumerable<IParticipantResolver> participantResolvers,
            IEnumerable<IUserPreferencesProvider> userPreferencesProviders,
            IEnumerable<IEntityDataItemProvider> entityDataItemProviders,
            IEnumerable<ITemplateIdProvider> templateIdProviders,
            IEnumerable<ICompositeDataItemProvider> compositeDataItemProviders,
            ILogger<CommunicationProcessor> logger)
        {
            foreach (var plugin in participantResolvers) _participantResolvers.Add(plugin.ParticipantResolverName, plugin);
            foreach (var plugin in userPreferencesProviders) _userPreferencesProviders.Add(plugin.UserType, plugin);
            foreach (var plugin in entityDataItemProviders) _entityDataItemProviders.Add(plugin.EntityType, plugin);
            foreach (var plugin in templateIdProviders) _templateIdProviders.Add(plugin.ProviderServiceName, plugin);
            foreach (var plugin in compositeDataItemProviders) _compositeDataItemProviders.Add(plugin.ProviderName, plugin);
            _logger = logger;
        }

        public async Task<IEnumerable<CommunicationMessage>> CreateMessagesAsync(CommunicationRequest request)
        {
            _logger.LogInformation($"Starting to create messages for request id: '{request.RequestId}' of type: '{request.RequestType}'");
            var recipientsTask = GetPotentialRecipientsAsync(request);
            var dataItemsTask = GetEntityDataItemsAsync(request);
            await Task.WhenAll(recipientsTask, dataItemsTask);

            var recipients = recipientsTask.Result;
            var dataItems = dataItemsTask.Result;
            _logger.LogInformation($"{dataItems.Count} data items found for request id: '{request.RequestId}'");

            var participants = await GetPreferencesForParticipantsAsync(request.RequestType, recipients);
            _logger.LogInformation($"Retrieved {participants.Count()} participants for request id: '{request.RequestId}'");

            var optedInParticipants = GetOptedInParticipants(participants);
            _logger.LogInformation($"{optedInParticipants.Count()} participants have opted in for request id: '{request.RequestId}'");

            var messages = GenerateCommunicationMessages(request, request.TemplateProviderName, dataItems, optedInParticipants);
            _logger.LogInformation($"{messages.Count()} messages generated for request id: '{request.RequestId}'");

            await SetMessageTemplateIdsAsync(messages);

            return messages;
        }

        private Task<IEnumerable<CommunicationUser>> GetPotentialRecipientsAsync(CommunicationRequest request)
        {
            if (!_participantResolvers.ContainsKey(request.ParticipantsResolverName))
                throw new UnknownRecipientResolverTypeException($"Unable to resolve recipient resolver named '{request.ParticipantsResolverName}'");

            var resolver = _participantResolvers[request.ParticipantsResolverName];
            return resolver.GetParticipantsAsync(request);
        }

        private async Task<List<CommunicationDataItem>> GetEntityDataItemsAsync(CommunicationRequest request)
        {
            var dataItems = new List<CommunicationDataItem>();

            foreach (var entity in request.Entities)
            {
                if (!_entityDataItemProviders.ContainsKey(entity.EntityType))
                    throw new EntityDataItemProviderNotFoundException($"Unable to resolve entity data item provider for entity type '{entity.EntityType}'");

                var entityDataItemProvider = _entityDataItemProviders[entity.EntityType];
                var entityDataItems = await entityDataItemProvider.GetDataItemsAsync(entity.EntityId);

                dataItems.AddRange(entityDataItems);
            }

            dataItems.AddRange(request.DataItems);

            return dataItems;
        }

        private async Task<IEnumerable<Participant>> GetPreferencesForParticipantsAsync(string requestType, IEnumerable<CommunicationUser> users)
        {
            var participants = new List<Participant>();

            foreach (var user in users)
            {
                var provider = _userPreferencesProviders[user.UserType];

                var recipientPreferenceForRequestType = await provider.GetUserPreferenceAsync(requestType, user);

                var participant = new Participant() { User = user, Preferences = recipientPreferenceForRequestType };

                participants.Add(participant);
            }

            return participants;
        }

        private async Task SetMessageTemplateIdsAsync(IEnumerable<CommunicationMessage> messages)
        {
            foreach (var message in messages)
            {
                if (!_templateIdProviders.ContainsKey(message.OriginatingServiceName))
                    throw new TemplateIdProviderNotFoundException(message.OriginatingServiceName);

                var templateIdProvider = _templateIdProviders[message.OriginatingServiceName];
                var templateId = await templateIdProvider.GetTemplateIdAsync(message);

                if (string.IsNullOrWhiteSpace(templateId))
                    throw new TemplateIdNotFoundException(message);

                message.TemplateId = templateId;
            }
        }

        private IEnumerable<CommunicationMessage> GenerateCommunicationMessages(
            CommunicationRequest request,
            string originatingService,
            List<CommunicationDataItem> dataItems,
            IEnumerable<Participant> filteredParticipants)
        {
            var messages = filteredParticipants.SelectMany(p =>
            {
                var channels = p.Preferences.Channels.ToDeliveryChannels();

                return channels.Select(channel => new CommunicationMessage
                    {
                        Id = Guid.NewGuid(),
                        RequestDateTime = request.RequestDateTime,
                        ParticipantsResolverName = request.ParticipantsResolverName,
                        OriginatingServiceName = originatingService,
                        Recipient = p.User,
                        DataItems = dataItems,
                        RequestType = request.RequestType,
                        Channel = channel,
                        Frequency = p.Preferences.Frequency,
                        Status = CommunicationMessageStatus.Unsent
                    }
                );
            });

            return messages.ToArray();
        }

        public static IEnumerable<Participant> GetOptedInParticipants(IEnumerable<Participant> participants)
        {
            return
                participants.Where(p =>
                    p.Preferences.Channels != DeliveryChannelPreferences.None
                    && !(p.Preferences.Scope == NotificationScope.Individual
                        && p.User.Participation == UserParticipation.SecondaryUser));
        }
    }
}