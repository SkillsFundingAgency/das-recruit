﻿using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories
{
    public interface IVacancyReviewRepository
    {
        Task CreateAsync(VacancyReview vacancy);
        Task<List<VacancyReview>> GetActiveAsync();
        Task<VacancyReview> GetAsync(Guid reviewId);
        Task UpdateAsync(VacancyReview review);
        Task<List<VacancyReview>> GetForVacancyAsync(long vacancyReference);
        Task<List<VacancyReviewSearch>> SearchAsync(long vacancyReference);
        Task<List<VacancyReview>> GetByStatusAsync(ReviewStatus status);
        Task<int> GetApprovedCountAsync(string submittedByUserId);
        Task<int> GetApprovedFirstTimeCountAsync(string submittedByUserId);
        Task<List<VacancyReview>> GetAssignedForUserAsync(string userId, DateTime assignationExpiryDateTime);
    }
}