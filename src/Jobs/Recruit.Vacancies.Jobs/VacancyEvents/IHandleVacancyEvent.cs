using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;

namespace Esfa.Recruit.Vacancies.Jobs.GenerateVacancyNumber
{
    internal interface IHandleVacancyEvent<T> where T : IVacancyEvent
    {
        Task Handle(T @event);
    }

}

