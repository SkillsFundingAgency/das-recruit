using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Qualifications;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public interface IQaVacancyClient
    {
        Task<QaDashboard> GetDashboardAsync();
        Task<Vacancy> GetVacancyAsync(long vacancyReference);
        Task<IApprenticeshipProgramme> GetApprenticeshipProgrammeAsync(string programmeId);
        Task ApproveReviewAsync(Guid reviewId);
        Task<VacancyReview> GetVacancyReviewAsync(Guid reviewId);
        Task StartReviewAsync(Guid reviewId, VacancyUser user);
        Task ReferVacancyReviewAsync(Guid reviewId);
        Task ApproveReferredReviewAsync(Guid reviewId, string shortDescription, string vacancyDescription, string trainingDescription, string outcomeDescription, string thingsToConsider, string employerDescription);
        Task<Qualifications> GetCandidateQualificationsAsync();
        Task<List<QaVacancySummary>> GetSearchResultsAsync(string searchTerm);
    }
}