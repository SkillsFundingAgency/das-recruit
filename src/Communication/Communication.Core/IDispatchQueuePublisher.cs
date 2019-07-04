using System.Threading.Tasks;
using Communication.Types;

namespace Communication.Core
{
    public interface IDispatchQueuePublisher
    {
        Task AddMessageAsync(CommunicationMessageIdentifier message);
    }
}