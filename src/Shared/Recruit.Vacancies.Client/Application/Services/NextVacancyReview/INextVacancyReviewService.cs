using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Services.NextVacancyReview
{
    public interface INextVacancyReviewService
    {
        Task<VacancyReview> GetNextVacancyReviewAsync(string userId);
        DateTime GetExpiredAssignationDateTime();
    }
}