using System;
using System.Collections.Generic;
using System.IO;
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
        Task<IApprenticeshipRoute> GetRoute(int? routeId);
        Task<VacancyReview> GetVacancyReviewAsync(Guid reviewId);
        Task ReferVacancyReviewAsync(Guid reviewId, string manualQaComment, List<ManualQaFieldIndicator> manualQaFieldIndicators, List<Guid> automatedQaRuleOutcomeIds);
        Task<Qualifications> GetCandidateQualificationsAsync();
        Task<VacancyReview> GetSearchResultAsync(string searchTerm);
        Task<int> GetApprovedCountAsync(string submittedByUserId);
        Task<List<VacancyReview>> GetVacancyReviewsInProgressAsync();
        Task<int> GetApprovedFirstTimeCountAsync(string submittedByUserId);
        Task AssignNextVacancyReviewAsync(VacancyUser user);
        Task AssignVacancyReviewAsync(VacancyUser user, Guid reviewId);
        Task<List<VacancyReview>> GetAssignedVacancyReviewsForUserAsync(string userId);
        bool VacancyReviewCanBeAssigned(VacancyReview review);
        bool VacancyReviewCanBeAssigned(ReviewStatus status, DateTime? reviewedDate);
        Task UnassignVacancyReviewAsync(Guid reviewId);
        Task<VacancyReview> GetCurrentReferredVacancyReviewAsync(long vacancyReference);
        Task<List<VacancyReview>> GetVacancyReviewHistoryAsync(long vacancyReference);
        Task<int> GetAnonymousApprovedCountAsync(string accountLegalEntityPublicHashedId);
        Task<Guid> CreateApplicationsReportAsync(DateTime fromDate, DateTime toDate, VacancyUser user, string reportName);
        Task<List<ReportSummary>> GetReportsAsync();
        Task<Report> GetReportAsync(Guid reportId);
        void WriteReportAsCsv(Stream stream, Report report);
        Task IncrementReportDownloadCountAsync(Guid reportId);
        Task UpdateDraftVacancyAsync(Vacancy vacancy, VacancyUser user);
    }
}