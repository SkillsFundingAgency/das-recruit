using System;
using System.Threading.Tasks;

namespace Communication.Types
{
    public interface ICommunicationRepository
    {
        Task InsertAsync(CommunicationMessage msg);
        Task<CommunicationMessage> GetAsync(Guid msgId);
    }
}