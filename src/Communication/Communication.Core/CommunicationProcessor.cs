using System.Collections.Generic;
using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Interfaces;

namespace Communication.Core
{
    public class CommunicationProcessor : ICommunicationProcessor
    {
        private readonly Dictionary<string, IParticipantResolver> _participantResolvers = new Dictionary<string, IParticipantResolver>();
        private readonly Dictionary<string, IUserPreferencesProvider> _userPreferencesProviders = new Dictionary<string, IUserPreferencesProvider>();
        private readonly Dictionary<string, IEntityDataItemProvider> _entityDataItemProviders = new Dictionary<string, IEntityDataItemProvider>();

        public CommunicationProcessor(
            IEnumerable<IParticipantResolver> participantResolvers,
            IEnumerable<IUserPreferencesProvider> userPreferencesProviders,
            IEnumerable<IEntityDataItemProvider> entityDataItemProviders)
        {
            foreach (var plugin in participantResolvers) _participantResolvers.Add(plugin.ResolverName, plugin);
            foreach (var plugin in userPreferencesProviders) _userPreferencesProviders.Add(plugin.ProviderName, plugin);
            foreach (var plugin in entityDataItemProviders) _entityDataItemProviders.Add(plugin.ProviderName, plugin);
        }

        public async Task<IEnumerable<CommunicationMessage>> CreateMessages(CommunicationRequest request)
        {
            var messages = new List<CommunicationMessage>();

            return await Task.FromResult(messages);
        }
    }
}