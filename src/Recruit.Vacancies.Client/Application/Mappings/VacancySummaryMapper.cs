﻿using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;

namespace Esfa.Recruit.Vacancies.Client.Application.Mappings
{
    internal class VacancySummaryMapper
    {
        internal static VacancySummary MapFromVacancy(Vacancy updatedVacancy)
        {
            return new VacancySummary
            {
                Id = updatedVacancy.Id,
                Title = updatedVacancy.Title,
                CreatedDate = updatedVacancy.CreatedDate,
                Status = updatedVacancy.Status,
                SubmittedDate = updatedVacancy.SubmittedDate
            };
        }
    }
}
