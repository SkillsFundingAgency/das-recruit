using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Communication.Types
{
    public interface ICommunicationRepository
    {
        Task InsertAsync(CommunicationMessage msg);
        Task<CommunicationMessage> GetAsync(Guid msgId);
        Task<IEnumerable<CommunicationMessage>> GetManyAsync(IEnumerable<Guid> msgIds);
        Task UpdateAsync(CommunicationMessage commMsg);
        Task<IEnumerable<CommunicationMessage>> GetScheduledMessagesSinceAsync(string requestType, DeliveryFrequency frequency, DateTime from, DateTime to);
        Task UpdateScheduledMessagesAsSentAsync(IEnumerable<Guid> msgIds);
    }
}