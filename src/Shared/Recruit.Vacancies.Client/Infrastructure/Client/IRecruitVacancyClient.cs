using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
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
        Task<List<VacancyApplication>> GetVacancyApplicationsAsync(long vacancyReference);
        Task UpdatePublishedVacancyAsync(Vacancy vacancy, VacancyUser user, LiveUpdateKind updateKind);
        Task<Guid> CloneVacancyAsync(Guid vacancyId, VacancyUser user, SourceOrigin sourceOrigin, DateTime startDate, DateTime closingDate);
        Task<string> GetEmployerNameAsync(Vacancy vacancy);
        Task<string> GetEmployerDescriptionAsync(Vacancy vacancy);
        Task<EmployerProfile> GetEmployerProfileAsync(string employerAccountId, string accountLegalEntityPublicHashedId);
        Task UpdateEmployerProfileAsync(EmployerProfile employerProfile, VacancyUser user);
        Task<VacancyAnalyticsSummary> GetVacancyAnalyticsSummaryAsync(long vacancyReference);
        Task<User> GetUsersDetailsAsync(string userId);
        Task<UserNotificationPreferences> GetUserNotificationPreferencesAsync(string vacancyUserId);
        Task UpdateUserNotificationPreferencesAsync(UserNotificationPreferences preferences);
        EntityValidationResult ValidateUserNotificationPreferences(UserNotificationPreferences preferences);
        Task<IEnumerable<string>> GetEmployerIdentifiersAsync(string userId);
        EntityValidationResult ValidateQualification(Qualification qualification);
        Task CloseVacancyAsync(Guid vacancyId, VacancyUser user, ClosureReason reason);
        Task<IApprenticeshipRoute> GetRoute(int? routeId);
        Task<IEnumerable<IApprenticeshipRoute>> GetApprenticeshipRoutes();
    }
}