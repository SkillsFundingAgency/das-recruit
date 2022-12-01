using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Interfaces;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider
{
    public class VacancyTaskListStatusService : IVacancyTaskListStatusService
    {
        public bool IsTaskListCompleted(ITaskListVacancy vacancy)
        {
            if(vacancy.VacancyType.GetValueOrDefault() == VacancyType.Apprenticeship)
            {
                return vacancy.HasSubmittedAdditionalQuestions = true;
            }
            if (vacancy.VacancyType == VacancyType.Traineeship)
            {
                return vacancy.HasChosenProviderContactDetails ?? false;
            }
                

            return false;
        }
    }
}