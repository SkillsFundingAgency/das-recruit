using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public interface IJobsVacancyClient
    {
        Task AssignVacancyNumber(Guid vacancyId);
        Task PatchTrainingProviderAsync(Guid vacancyId);
        Task UpdateApprenticeshipProgrammesAsync();
        Task<IEnumerable<LegalEntity>> GetEmployerLegalEntitiesAsync(string employerAccountId);
        Task CreateVacancyReview(long vacancyReference);
        Task<IEnumerable<LiveVacancy>> GetLiveVacancies();
        Task CloseExpiredVacancies();
        Task EnsureVacancyIsGeocodedAsync(Guid vacancyId);
        Task ApproveVacancy(long vacancyReference);
        Task UpdateBankHolidaysAsync();
        Task ReferVacancy(long vacancyReference);
        Task CreateApplicationReviewAsync(Domain.Entities.Application application);
        Task PerformRulesCheckAsync(Guid reviewId);
        Task WithdrawApplicationAsync(long vacancyReference, Guid candidateId);
        Task HardDeleteApplicationReviewsForCandidate(Guid candidateId);
        Task RefreshEmployerProfiles(string employerAccountId, IEnumerable<long> legalEntities);
    }
}