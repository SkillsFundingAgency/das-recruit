using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents
{
    public interface IDomainEventHandler<out T> where T : IEvent
    {
        Task HandleAsync(string eventPayload);
    }
}

