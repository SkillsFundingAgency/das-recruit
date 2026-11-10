using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Queues;

public interface IRecruitQueueService
{
    Task AddMessageAsync<T>(T message);
}