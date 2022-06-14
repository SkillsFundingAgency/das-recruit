using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Interfaces;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider
{
    public class VacancyTaskListStatusService : IVacancyTaskListStatusService
    {
        public bool IsTaskListCompleted(ITaskListVacancy vacancy)
        {
            if(vacancy.VacancyType == VacancyType.Apprenticeship)
                return vacancy.ApplicationMethod != null;
            if (vacancy.VacancyType == VacancyType.Traineeship)
                return !string.IsNullOrEmpty(vacancy.EmployerDescription);

            return false;
        }
    }
}