using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public interface IRecruitVacancyClient
    {
        Task AssignVacancyNumber(Guid vacancyId);
        Task UserSignedInAsync(VacancyUser user, UserType userType);
        Task<Vacancy> GetVacancyAsync(Guid vacancyId);
        Task<VacancyReview> GetCurrentReferredVacancyReviewAsync(long vacancyReference);
        Task<List<string>> GetCandidateSkillsAsync();
        Task<IList<string>> GetCandidateQualificationsAsync();
        Task<ApplicationReview> GetApplicationReviewAsync(Guid applicationReviewId);
        EntityValidationResult Validate(Vacancy vacancy, VacancyRuleSet rules);
        Task UpdateDraftVacancyAsync(Vacancy vacancy, VacancyUser user);
        Task<IEnumerable<IApprenticeshipProgramme>> GetActiveApprenticeshipProgrammesAsync();
        Task<IApprenticeshipProgramme> GetApprenticeshipProgrammeAsync(string programmeId);
        Task<List<VacancyApplication>> GetVacancyApplicationsAsync(long vacancyReference, bool vacancySharedByProvider = false);
        Task<List<VacancyApplication>> GetVacancyApplicationsSortedAsync(long vacancyReference, SortColumn sortColumn, SortOrder sortOrder, bool vacancySharedByProvider = false);
        Task<List<VacancyApplication>> GetVacancyApplicationsForSelectedIdsAsync(List<Guid> applicationReviewIds);
        Task<List<VacancyApplication>> GetVacancyApplicationsForReferenceAndStatus(Guid vacancyId, ApplicationReviewStatus status);
        Task UpdatePublishedVacancyAsync(Vacancy vacancy, VacancyUser user, LiveUpdateKind updateKind);
        Task<Guid> CloneVacancyAsync(Guid vacancyId, VacancyUser user, SourceOrigin sourceOrigin, DateTime startDate, DateTime closingDate);
        Task<string> GetEmployerNameAsync(Vacancy vacancy);
        Task<string> GetEmployerDescriptionAsync(Vacancy vacancy);
        Task<EmployerProfile> GetEmployerProfileAsync(string employerAccountId, string accountLegalEntityPublicHashedId);
        Task UpdateEmployerProfileAsync(EmployerProfile employerProfile, VacancyUser user);
        Task<VacancyAnalyticsSummary> GetVacancyAnalyticsSummaryAsync(long vacancyReference);
        Task<User> GetUsersDetailsAsync(string userId);
        Task UpsertUserDetails(User user);
        Task<UserNotificationPreferences> GetUserNotificationPreferencesAsync(string vacancyUserId, string dfeUserId = null);

        Task<UserNotificationPreferences> GetUserNotificationPreferencesByDfEUserIdAsync(string idamsUserId, string dfeUserId = null);
        Task UpdateUserNotificationPreferencesAsync(UserNotificationPreferences preferences);
        EntityValidationResult ValidateUserNotificationPreferences(UserNotificationPreferences preferences);
        Task<GetUserAccountsResponse> GetEmployerIdentifiersAsync(string userId, string email);
        EntityValidationResult ValidateQualification(Qualification qualification);
        Task CloseVacancyAsync(Guid vacancyId, VacancyUser user, ClosureReason reason);
        Task SetApplicationReviewsStatus(long vacancyReference, IEnumerable<Guid> applicationReviewIds, VacancyUser user, ApplicationReviewStatus? status, Guid vacancyId, ApplicationReviewStatus? applicationReviewTemporaryStatus);
        Task SetApplicationReviewsPendingUnsuccessfulFeedback(VacancyUser user, ApplicationReviewStatus status, Guid vacancyId, string feedback);
        Task SetApplicationReviewsToUnsuccessful(IEnumerable<Guid> applicationReviewsToUnsuccessful, string candidateFeedback, VacancyUser user, Guid vacancyId);
        Task <User> GetUsersDetailsByDfEUserId(string dfeUserId);
    }
}