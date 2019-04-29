using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface INotifyVacancyUpdates
    {
        Task VacancyManuallyClosed(Vacancy vacancy);
        Task LiveVacancyChanged(Vacancy vacancy);
    }
}