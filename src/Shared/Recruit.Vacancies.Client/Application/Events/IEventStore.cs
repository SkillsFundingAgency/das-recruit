using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Events;

public interface IEventStore
{
    Task Add(IEvent @event);
}