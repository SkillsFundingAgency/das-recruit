using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Queues
{
    public interface IReportsQueue
    {
        Task AddAsync(Guid reportId);
    }
}
