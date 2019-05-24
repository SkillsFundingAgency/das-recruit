using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Queues
{
    public interface IQueue
    {
        Task AddMessageAsync<T>(T message);
    }
}
