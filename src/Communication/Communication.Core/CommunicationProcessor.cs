using System.Collections.Generic;
using System.Threading.Tasks;
using Communication.Types.Exceptions;
using Communication.Types;
using Communication.Types.Extensions;
using Communication.Types.Interfaces;
using System.Linq;
using System;

namespace Communication.Core
{
    public class CommunicationProcessor : ICommunicationProcessor
    {
        private readonly Dictionary<string, IParticipantResolver> _participantResolvers = new Dictionary<string, IParticipantResolver>();
        private readonly Dictionary<string, IUserPreferencesProvider> _userPreferencesProviders = new Dictionary<string, IUserPreferencesProvider>();
        private readonly Dictionary<string, IEntityDataItemProvider> _entityDataItemProviders = new Dictionary<string, IEntityDataItemProvider>();
        private readonly Dictionary<string, ITemplateIdProvider> _templateIdProviders = new Dictionary<string, ITemplateIdProvider>();

        public CommunicationProcessor(
            IEnumerable<IParticipantResolver> participantResolvers,
            IEnumerable<IUserPreferencesProvider> userPreferencesProviders,
            IEnumerable<IEntityDataItemProvider> entityDataItemProviders,
            IEnumerable<ITemplateIdProvider> templateIdProviders)
        {
            foreach (var plugin in participantResolvers) _participantResolvers.Add(plugin.ParticipantResolverName, plugin);
            foreach (var plugin in userPreferencesProviders) _userPreferencesProviders.Add(plugin.UserType, plugin);
            foreach (var plugin in entityDataItemProviders) _entityDataItemProviders.Add(plugin.EntityType, plugin);
            foreach (var plugin in templateIdProviders) _templateIdProviders.Add(plugin.ProviderServiceName, plugin);
        }

        public async Task<IEnumerable<CommunicationMessage>> CreateMessagesAsync(CommunicationRequest request)
        {
            var recipientsTask = GetPotentialRecipientsAsync(request);
            var dataItemsTask = GetEntityDataItemsAsync(request);
            await Task.WhenAll(recipientsTask, dataItemsTask);

            var recipients = recipientsTask.Result;
            var dataItems = dataItemsTask.Result;

            var participants = await GetPreferencesForParticipantsAsync(request.RequestType, recipients);

            var optedInParticipants = GetOptedInParticipants(participants);

            var messages = GenerateCommunicationMessages(request, request.TemplateProviderName, dataItems, optedInParticipants);

            await SetMessageTemplateIds(messages);

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
                var entityDataItems = await entityDataItemProvider.GetDataItems(entity.EntityId);

                dataItems.AddRange(entityDataItems);
            }

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

        private async Task SetMessageTemplateIds(IEnumerable<CommunicationMessage> messages)
        {
            foreach (var message in messages)
            {
                if (!_templateIdProviders.ContainsKey(message.OriginatingServiceName))
                    throw new TemplateIdProviderNotFoundException(message.OriginatingServiceName);

                var templateIdProvider = _templateIdProviders[message.OriginatingServiceName];
                var templateId = await templateIdProvider.GetTemplateId(message);

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