using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Queues
{
    public interface IQueueService
    {
        Task AddMessageAsync<T>(T message);
    }
}
