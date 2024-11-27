using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Interfaces;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider
{
    public class VacancyTaskListStatusService : IVacancyTaskListStatusService
    {
        public bool IsTaskListCompleted(ITaskListVacancy vacancy)
        {
            if (vacancy.OwnerType == OwnerType.Provider)
            {
                return vacancy.HasSubmittedAdditionalQuestions;
            }

            if (vacancy.OwnerType == OwnerType.Employer)
            {
                return vacancy.HasSubmittedAdditionalQuestions;
            }

            return false;
        }
    }
}