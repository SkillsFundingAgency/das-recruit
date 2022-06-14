using Esfa.Recruit.Vacancies.Client.Domain.Interfaces;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider
{
    public interface IVacancyTaskListStatusService
    {
        bool IsTaskListCompleted(ITaskListVacancy vacancy);
    }
}