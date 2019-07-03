using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue
{
    public interface ICommunicationQueueService
    {
        Task AddMessageAsync<T>(T message);
    }
}