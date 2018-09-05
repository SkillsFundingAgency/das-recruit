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
        Task ApproveVacancyReviewAsync(Guid reviewId, string reviewerComment);
        Task<VacancyReview> GetVacancyReviewAsync(Guid reviewId);
        Task StartReviewAsync(Guid reviewId, VacancyUser user);
        Task ReferVacancyReviewAsync(Guid reviewId);
        Task ApproveReferredReviewAsync(Guid reviewId, string shortDescription, string vacancyDescription, string trainingDescription, string outcomeDescription, string thingsToConsider, string employerDescription);
        Task<Qualifications> GetCandidateQualificationsAsync();
        Task<List<VacancyReviewSearch>> GetSearchResultsAsync(string searchTerm);
        Task<VacancyReview> AssignNextVacancyReviewAsync(VacancyUser user);
        Task<int> GetApprovedCountAsync(string submittedByUserId);
        Task<int> GetApprovedFirstTimeCountAsync(string submittedByUserId);
    }
}