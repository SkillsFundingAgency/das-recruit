using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories
{
    public interface IApplicationReviewRepository
    {
        Task CreateAsync(ApplicationReview review);
        Task<ApplicationReview> GetAsync(Guid applicationReviewId);
        Task<ApplicationReview> GetAsync(long vacancyReference, Guid candidateId);
        Task UpdateAsync(ApplicationReview applicationReview);
        Task<List<ApplicationReview>> GetForCandidateAsync(Guid candidateId);
        Task HardDelete(Guid applicationReviewId);
    }
}
