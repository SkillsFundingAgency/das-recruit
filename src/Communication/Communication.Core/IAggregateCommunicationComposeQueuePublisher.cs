using System.Threading.Tasks;
using Communication.Types;

namespace Communication.Core
{
    public interface IAggregateCommunicationComposeQueuePublisher
    {
        Task AddMessageAsync(AggregateCommunicationComposeRequest message);
    }
}