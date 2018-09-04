﻿using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Providers
{
    public interface INextVacancyReviewProvider
    {
        Task<VacancyReview> GetNextVacancyReviewAsync(string userId);
    }
}