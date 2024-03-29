﻿using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Interfaces;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider
{
    public class VacancyTaskListStatusService : IVacancyTaskListStatusService
    {
        public bool IsTaskListCompleted(ITaskListVacancy vacancy)
        {
            if (vacancy.OwnerType == OwnerType.Provider)
            {
                if (vacancy.VacancyType.GetValueOrDefault() == VacancyType.Apprenticeship)
                {
                    return vacancy.HasSubmittedAdditionalQuestions;
                }
                if (vacancy.VacancyType == VacancyType.Traineeship)
                {
                    return vacancy.HasChosenProviderContactDetails ?? false;
                }
            }
            else if (vacancy.OwnerType == OwnerType.Employer)
            {
                return vacancy.HasSubmittedAdditionalQuestions;
            }
                
            return false;
        }
    }
}