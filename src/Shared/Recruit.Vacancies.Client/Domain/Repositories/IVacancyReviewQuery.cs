using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories;

public interface IVacancyReviewQuery
{
    Task<List<VacancyReview>> GetForVacancyAsync(long vacancyReference);
    Task<VacancyReview> GetCurrentReferredVacancyReviewAsync(long vacancyReference);
}