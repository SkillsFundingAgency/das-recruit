using System.Collections.Generic;
using System.Threading.Tasks;
using Communication.Types.Exceptions;
using Communication.Types;
using Communication.Types.Interfaces;
using System.Linq;

namespace Communication.Core
{
    public class AggregateCommunicationProcessor : IAggregateCommunicationProcessor
    {
        private readonly IDictionary<string, IParticipantResolver> _participantResolvers = new Dictionary<string, IParticipantResolver>();
        private readonly IDictionary<string, IUserPreferencesProvider> _userPreferencesProviders = new Dictionary<string, IUserPreferencesProvider>();
        private readonly IDictionary<string, IEntityDataItemProvider> _entityDataItemProviders = new Dictionary<string, IEntityDataItemProvider>();
        private readonly IDictionary<string, ITemplateIdProvider> _templateIdProviders = new Dictionary<string, ITemplateIdProvider>();
        private readonly IDictionary<string, ICompositeDataItemProvider> _compositeDataItemProviders = new Dictionary<string, ICompositeDataItemProvider>();

        public AggregateCommunicationProcessor(
            IEnumerable<IParticipantResolver> participantResolvers,
            IEnumerable<IUserPreferencesProvider> userPreferencesProviders,
            IEnumerable<IEntityDataItemProvider> entityDataItemProviders,
            IEnumerable<ITemplateIdProvider> templateIdProviders,
            IEnumerable<ICompositeDataItemProvider> compositeDataItemProviders)
        {
            foreach (var plugin in participantResolvers) _participantResolvers.Add(plugin.ParticipantResolverName, plugin);
            foreach (var plugin in userPreferencesProviders) _userPreferencesProviders.Add(plugin.UserType, plugin);
            foreach (var plugin in entityDataItemProviders) _entityDataItemProviders.Add(plugin.EntityType, plugin);
            foreach (var plugin in templateIdProviders) _templateIdProviders.Add(plugin.ProviderServiceName, plugin);

            foreach (var plugin in compositeDataItemProviders) _compositeDataItemProviders.Add(plugin.ProviderName, plugin);
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

        public async Task<CommunicationMessage> CreateAggregateMessageAsync(AggregateCommunicationRequest request, IEnumerable<CommunicationMessage> messages)
        {
            var recipient = GetUserFromAggregateMessage(messages);

            var participant = (await GetPreferencesForParticipantsAsync(request.RequestType, new[] {recipient})).Single();

            if (participant.Preferences.Channels == DeliveryChannelPreferences.None)
                return null;

            var validMessages = await GetValidatedAggregateMessagesAsync(messages);

            if (!validMessages.Any())
                return null;

            var message = await GenerateMessageForAggregateMessagesAsync(request, validMessages);

            return message;
        }

        private CommunicationUser GetUserFromAggregateMessage(IEnumerable<CommunicationMessage> messages)
        {
            var templateMessage = GetTemplateMessageFromMessagesToBeAggregated(messages);

            return new CommunicationUser(templateMessage.Recipient.UserId, templateMessage.Recipient.Email, templateMessage.Recipient.Name, templateMessage.Recipient.UserType, UserParticipation.PrimaryUser);
        }

        private CommunicationMessage GetTemplateMessageFromMessagesToBeAggregated(IEnumerable<CommunicationMessage> messages) => messages.First();

        private async Task<IEnumerable<CommunicationMessage>> GetValidatedAggregateMessagesAsync(IEnumerable<CommunicationMessage> messages)
        {
            var recipientsResolverName = messages.First().ParticipantsResolverName;

            if (!_participantResolvers.ContainsKey(recipientsResolverName))
                throw new UnknownRecipientResolverTypeException($"Unable to resolve recipient resolver named '{recipientsResolverName}'");

            var recipientsResolver = _participantResolvers[recipientsResolverName];

            var validatedMessages = await recipientsResolver.ValidateParticipantAsync(messages);

            return validatedMessages;
        }

        private async Task<CommunicationMessage> GenerateMessageForAggregateMessagesAsync(AggregateCommunicationRequest request, IEnumerable<CommunicationMessage> messages)
        {
            var templateMessage = GetTemplateMessageFromMessagesToBeAggregated(messages);

            var aggregatedMessage = new CommunicationMessage
            {
                Id = request.RequestId,
                RequestDateTime = request.RequestDateTime,
                ParticipantsResolverName = templateMessage.ParticipantsResolverName,
                OriginatingServiceName = templateMessage.OriginatingServiceName,
                Recipient = new CommunicationUser(templateMessage.Recipient.UserId, templateMessage.Recipient.Email, templateMessage.Recipient.Name, templateMessage.Recipient.UserType, UserParticipation.PrimaryUser),
                RequestType = request.RequestType,
                Channel = templateMessage.Channel,
                Frequency = DeliveryFrequency.Immediate,
                TemplateId = templateMessage.TemplateId
            };

            var compositeDataItemProvider = _compositeDataItemProviders[templateMessage.RequestType];

            var consolidatedMessageData = await compositeDataItemProvider.GetConsolidatedMessageDataItemsAsync(aggregatedMessage, messages);

            if (consolidatedMessageData != null && consolidatedMessageData.Any())
            {
                aggregatedMessage.DataItems = consolidatedMessageData.ToList();
            }

            return aggregatedMessage;
        }
    }
}