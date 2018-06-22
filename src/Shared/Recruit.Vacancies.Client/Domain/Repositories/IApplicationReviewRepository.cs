using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories
{
    public interface IApplicationReviewRepository
    {
        Task CreateAsync(ApplicationReview review);
        Task<List<T>> GetApplicationReviewsForEmployerAsync<T>(string employerAccountId);
        Task<List<T>> GetApplicationReviewsForVacancyAsync<T>(long vacancyReference);
        Task<T> GetApplicationReviewAsync<T>(Guid applicationReviewId);
    }
}
