using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services.Reports;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Configuration;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public partial class VacancyClient(
        ILogger<VacancyClient> logger,
        IVacancyRepository repository,
        IVacancyQuery vacancyQuery,
        IQueryStoreReader reader,
        IMessaging messaging,
        IEntityValidator<Vacancy, VacancyRuleSet> validator,
        IApprenticeshipProgrammeProvider apprenticeshipProgrammesProvider,
        IEmployerAccountProvider employerAccountProvider,
        IApplicationReviewRepository applicationReviewRepository,
        IVacancyReviewQuery vacancyReviewQuery,
        ICandidateSkillsProvider candidateSkillsProvider,
        IVacancyService vacancyService,
        IEmployerProfileRepository employerProfileRepository,
        IUserRepository userRepository,
        IQualificationsProvider qualificationsProvider,
        IEmployerService employerService,
        IReportRepository reportRepository,
        IReportService reportService,
        IUserNotificationPreferencesRepository userNotificationPreferencesRepository,
        AbstractValidator<UserNotificationPreferences> userNotificationPreferencesValidator,
        AbstractValidator<Qualification> qualificationValidator,
        IVacancySummariesProvider vacancySummariesQuery,
        ITimeProvider timeProvider,
        ITrainingProviderService trainingProviderService,
        IFeature feature)
        : IRecruitVacancyClient, IEmployerVacancyClient, IJobsVacancyClient
    {
        private readonly bool _isMongoMigrationFeatureEnabled = feature.IsFeatureEnabled(FeatureNames.MongoMigration);

        public Task UpdateDraftVacancyAsync(Vacancy vacancy, VacancyUser user)
        {
            var command = new UpdateDraftVacancyCommand
            {
                Vacancy = vacancy,
                User = user
            };

            return messaging.SendCommandAsync(command);
        }

        public Task UpdatePublishedVacancyAsync(Vacancy vacancy, VacancyUser user, LiveUpdateKind updateKind)
        {
            var command = new UpdateLiveVacancyCommand(vacancy, user, updateKind);

            return messaging.SendCommandAsync(command);
        }

        public Task<Vacancy> GetVacancyAsync(Guid vacancyId)
        {
            return repository.GetVacancyAsync(vacancyId);
        }

        public Task<string> GetEmployerNameAsync(Vacancy vacancy)
        {
            return employerService.GetEmployerNameAsync(vacancy);
        }

        public Task<string> GetEmployerDescriptionAsync(Vacancy vacancy)
        {
            return employerService.GetEmployerDescriptionAsync(vacancy);
        }

        public async Task<Guid> CreateVacancyAsync(string title, string employerAccountId, VacancyUser user, TrainingProvider provider = null, string programmeId = null)
        {
            var vacancyId = GenerateVacancyId();

            var command = new CreateEmployerOwnedVacancyCommand
            {
                VacancyId = vacancyId,
                User = user,
                UserType = UserType.Employer,
                Title = title,
                EmployerAccountId = employerAccountId,
                Origin = SourceOrigin.EmployerWeb
            };
            if (provider != null)
                command.TrainingProvider = provider;

            if (programmeId != null)
                command.ProgrammeId = programmeId;

            await messaging.SendCommandAsync(command);

            return vacancyId;
        }

        public async Task CreateEmployerApiVacancy(Guid id, string title, string employerAccountId, VacancyUser user,
            TrainingProvider provider, string programmeId)
        {
            var command = new CreateEmployerOwnedVacancyCommand
            {
                VacancyId = id,
                User = user,
                UserType = UserType.Employer,
                Title = title,
                EmployerAccountId = employerAccountId,
                Origin = SourceOrigin.Api,
                ProgrammeId = programmeId,
                TrainingProvider = provider
            };
            
            await messaging.SendCommandAsync(command);
            
            await AssignVacancyNumber(id);
        }

        public async Task<Guid> CloneVacancyAsync(
            Guid vacancyId, VacancyUser user, SourceOrigin sourceOrigin,
            DateTime startDate, DateTime closingDate)
        {
            var newVacancyId = GenerateVacancyId();

            var command = new CloneVacancyCommand(vacancyId, newVacancyId, user, sourceOrigin, startDate, closingDate);

            await messaging.SendCommandAsync(command);

            await AssignVacancyNumber(newVacancyId);

            return newVacancyId;
        }

        private Guid GenerateVacancyId()
        {
            return Guid.NewGuid();
        }

        public Task DeleteVacancyAsync(Guid vacancyId, VacancyUser user)
        {
            var command = new DeleteVacancyCommand
            {
                VacancyId = vacancyId,
                User = user
            };

            return messaging.SendCommandAsync(command);
        }
        
        public async Task<EmployerDashboardSummary> GetDashboardSummary(string employerAccountId)
        {
            var dashboardValue = await  vacancySummariesQuery.GetEmployerOwnedVacancyDashboardByEmployerAccountIdAsync(employerAccountId);
            var dashboardStats = await employerAccountProvider.GetEmployerDashboardStats(employerAccountId);
            
            var dashboard = dashboardValue.VacancyStatusDashboard;
            var dashboardApplications = dashboardValue.VacancyApplicationsDashboard;
            var dashboardSharedApplications = dashboardValue.VacancySharedApplicationsDashboard;

            return new EmployerDashboardSummary
            {
                Closed = dashboard.FirstOrDefault(c=>c.Status == VacancyStatus.Closed)?.StatusCount ?? 0,
                Draft = dashboard.SingleOrDefault(c=>c.Status == VacancyStatus.Draft)?.StatusCount ?? 0,
                Review = _isMongoMigrationFeatureEnabled 
                    ? dashboardStats.EmployerReviewedApplicationsCount 
                    : dashboard.SingleOrDefault(c=>c.Status == VacancyStatus.Review)?.StatusCount ?? 0,
                Referred = (dashboard.SingleOrDefault(c=>c.Status == VacancyStatus.Referred)?.StatusCount ?? 0) + (dashboard.SingleOrDefault(c=>c.Status == VacancyStatus.Rejected)?.StatusCount ?? 0),
                Live = dashboard.Where(c=>c.Status == VacancyStatus.Live).Sum(c=>c.StatusCount),
                Submitted = dashboard.SingleOrDefault(c=>c.Status == VacancyStatus.Submitted)?.StatusCount ?? 0,
                NumberOfNewApplications = _isMongoMigrationFeatureEnabled 
                    ? dashboardStats.NewApplicationsCount 
                    : dashboardApplications.Where(c=>c.Status == VacancyStatus.Live || c.Status == VacancyStatus.Closed).Sum(x=>x.NoOfNewApplications),
                NumberOfSuccessfulApplications = dashboardApplications.Where(c=>c.Status == VacancyStatus.Live).Sum(x=>x.NoOfSuccessfulApplications) 
                                                 + dashboardApplications.Where(c=>c.Status == VacancyStatus.Closed).Sum(x=>x.NoOfSuccessfulApplications),
                NumberOfUnsuccessfulApplications = dashboardApplications.Where(c=>c.Status == VacancyStatus.Live).Sum(x=>x.NoOfUnsuccessfulApplications) 
                                                   + dashboardApplications.Where(c=>c.Status == VacancyStatus.Closed).Sum(x=>x.NoOfUnsuccessfulApplications),
                NumberOfSharedApplications = dashboardSharedApplications.Where(c => c.Status == VacancyStatus.Live || c.Status == VacancyStatus.Closed).Sum(x => x.NoOfSharedApplications),
                NumberOfAllSharedApplications = dashboardSharedApplications.Where(c => c.Status == VacancyStatus.Live || c.Status == VacancyStatus.Closed).Sum(x => x.NoOfAllSharedApplications),
                NumberClosingSoon =dashboard.FirstOrDefault(c=>c.Status == VacancyStatus.Live && c.ClosingSoon)?.StatusCount ?? 0,
                NumberClosingSoonWithNoApplications = dashboardValue.VacanciesClosingSoonWithNoApplications
            };
        }

        public async Task<EmployerDashboard> GetDashboardAsync(string employerAccountId, int page, FilteringOptions? status = null, string searchTerm = null)
        {
            var vacancySummaries =
                await vacancySummariesQuery.GetEmployerOwnedVacancySummariesByEmployerAccountId(employerAccountId,
                     page, status, searchTerm);
            return new EmployerDashboard
            {
                Id = QueryViewType.EmployerDashboard.GetIdValue(employerAccountId),
                Vacancies = vacancySummaries,
                LastUpdated = timeProvider.Now
            };
        }

        public Task UserSignedInAsync(VacancyUser user, UserType userType)
        {
            var command = new UserSignedInCommand(user, userType);

            return messaging.SendCommandAsync(command);
        }

        public Task SetupEmployerAsync(string employerAccountId)
        {
            var command = new SetupEmployerCommand
            {
                EmployerAccountId = employerAccountId
            };

            return messaging.SendCommandAsync(command);
        }

        public Task<EmployerEditVacancyInfo> GetEditVacancyInfoAsync(string employerAccountId)
        {
            return reader.GetEmployerVacancyDataAsync(employerAccountId);
        }

        public EntityValidationResult Validate(Vacancy vacancy, VacancyRuleSet rules)
        {
            return validator.Validate(vacancy, rules);
        }

        public Task<IEnumerable<IApprenticeshipProgramme>> GetActiveApprenticeshipProgrammesAsync()
        {
            return apprenticeshipProgrammesProvider.GetApprenticeshipProgrammesAsync();
        }

        public Task<IApprenticeshipProgramme> GetApprenticeshipProgrammeAsync(string programmeId)
        {
            return apprenticeshipProgrammesProvider.GetApprenticeshipProgrammeAsync(programmeId);
        }

        public Task<GetUserAccountsResponse> GetEmployerIdentifiersAsync(string userId, string email)
        {
            return employerAccountProvider.GetEmployerIdentifiersAsync(userId, email);
        }

        public Task<List<string>> GetCandidateSkillsAsync()
        {
            return candidateSkillsProvider.GetCandidateSkillsAsync();
        }

        public Task<IList<string>> GetCandidateQualificationsAsync()
        {
            return qualificationsProvider.GetQualificationsAsync();
        }

        public Task<ApplicationReview> GetApplicationReviewAsync(Guid applicationReviewId)
        {
            return applicationReviewRepository.GetAsync(applicationReviewId);
        }

        public async Task<List<VacancyApplication>> GetVacancyApplicationsSortedAsync(long vacancyReference, SortColumn sortColumn, SortOrder sortOrder, bool vacancySharedByProvider = false)
        {
            var applicationReviews = vacancySharedByProvider
                ? await applicationReviewRepository.GetForSharedVacancySortedAsync(vacancyReference, sortColumn, sortOrder)
                : await applicationReviewRepository.GetForVacancySortedAsync(vacancyReference, sortColumn, sortOrder);

            return applicationReviews == null
                ? new List<VacancyApplication>()
                : applicationReviews.Select(c => (VacancyApplication)c).ToList();
        }

        public async Task<List<VacancyApplication>> GetVacancyApplicationsAsync(long vacancyReference, bool vacancySharedByProvider = false)
        {
            var applicationReviews = vacancySharedByProvider
                ? await applicationReviewRepository.GetForSharedVacancyAsync(vacancyReference) 
                : await applicationReviewRepository.GetForVacancyAsync<ApplicationReview>(vacancyReference);

            return applicationReviews == null 
                ? new List<VacancyApplication>() 
                : applicationReviews.Select(c=>(VacancyApplication)c).ToList();
        }

        public async Task<List<VacancyApplication>> GetVacancyApplicationsForSelectedIdsAsync(List<Guid> applicationReviewIds)
        {
            var applicationReviews =
                await applicationReviewRepository.GetAllForSelectedIdsAsync<ApplicationReview>(applicationReviewIds);

            return applicationReviews == null
                ? new List<VacancyApplication>()
                : applicationReviews.Select(c => (VacancyApplication)c).ToList();
        }

        public async Task<List<VacancyApplication>> GetVacancyApplicationsForReferenceAndStatus(Guid vacancyId, ApplicationReviewStatus status)
        {
            var vacancy = await repository.GetVacancyAsync(vacancyId);
            
            var applicationReviews =
                await applicationReviewRepository.GetAllForVacancyWithTemporaryStatus(vacancy.VacancyReference!.Value!, status);

            return applicationReviews == null
                ? new List<VacancyApplication>()
                : applicationReviews.Select(c => (VacancyApplication)c).ToList();
        }

        public Task<bool> SetApplicationReviewStatus(Guid applicationReviewId, ApplicationReviewStatus? outcome, string candidateFeedback, VacancyUser user)
        {
            var command = new ApplicationReviewStatusEditCommand
            {
                ApplicationReviewId = applicationReviewId,
                Outcome = outcome,
                CandidateFeedback = candidateFeedback,
                User = user
            };

            return messaging.SendStatusCommandAsync(command);
        }

        public Task SetApplicationReviewsStatus(long vacancyReference, IEnumerable<Guid> applicationReviews, VacancyUser user, ApplicationReviewStatus? status, Guid vacancyId, ApplicationReviewStatus? applicationReviewTemporaryStatus)
        {
            var command = new ApplicationReviewsSharedCommand
            {
                ApplicationReviews = applicationReviews,
                User = user,
                Status = status,
                VacancyId = vacancyId,
                TemporaryStatus = applicationReviewTemporaryStatus,
                VacancyReference = vacancyReference
            };

            return messaging.SendCommandAsync(command);
        }

        public Task SetApplicationReviewsPendingUnsuccessfulFeedback(VacancyUser user, ApplicationReviewStatus status, Guid vacancyId, string feedback)
        {
            var command = new ApplicationReviewPendingUnsuccessfulFeedbackCommand
            {
                Feedback = feedback,
                User = user,
                Status = status,
                VacancyId = vacancyId
            };

            return messaging.SendCommandAsync(command);
        }

        public Task SetApplicationReviewsToUnsuccessful(IEnumerable<Guid> applicationReviewsToUnsuccessful, string candidateFeedback, VacancyUser user, Guid vacancyId)
        {
            var command = new ApplicationReviewsUnsuccessfulCommand
            {
                ApplicationReviews = applicationReviewsToUnsuccessful,
                CandidateFeedback = candidateFeedback,
                User = user,
                VacancyId = vacancyId,
                Status = ApplicationReviewStatus.Unsuccessful
            };

            return messaging.SendCommandAsync(command);
        }

        public Task<EmployerProfile> GetEmployerProfileAsync(string employerAccountId, string accountLegalEntityPublicHashedId)
        {
            return employerProfileRepository.GetAsync(employerAccountId, accountLegalEntityPublicHashedId);
        }

        public Task UpdateEmployerProfileAsync(EmployerProfile employerProfile, VacancyUser user)
        {
            var command = new UpdateEmployerProfileCommand
            {
                Profile = employerProfile,
                User = user
            };

            return messaging.SendCommandAsync(command);
        }

        // Jobs
        public Task AssignVacancyNumber(Guid vacancyId)
        {
            var command = new AssignVacancyNumberCommand
            {
                VacancyId = vacancyId
            };

            return messaging.SendCommandAsync(command);
        }

        public Task PatchTrainingProviderAsync(Guid vacancyId)
        {
            var command = new PatchVacancyTrainingProviderCommand(vacancyId);

            return messaging.SendCommandAsync(command);
        }

        public Task UpdateProviders()
        {
            var command = new UpdateProvidersCommand();

            return messaging.SendCommandAsync(command);
        }


        public Task UpdateBankHolidaysAsync()
        {
            var command = new UpdateBankHolidaysCommand();

            return messaging.SendCommandAsync(command);
        }

        public async Task CreateVacancyReview(long vacancyReference)
        {
            var command = new CreateVacancyReviewCommand
            {
                VacancyReference = vacancyReference
            };

            await messaging.SendCommandAsync(command);
        }

        public Task CloseVacancyAsync(Guid vacancyId, VacancyUser user, ClosureReason reason)
        {
            return messaging.SendCommandAsync(new CloseVacancyCommand(vacancyId, user, reason));
        }

        public async Task CloseExpiredVacancies()
        {
            var command = new CloseExpiredVacanciesCommand();

            await messaging.SendCommandAsync(command);
        }

        public async Task EnsureVacancyIsGeocodedAsync(Guid vacancyId)
        {
            var command = new GeocodeVacancyCommand { VacancyId = vacancyId };
            await messaging.SendCommandAsync(command);
        }

        public Task ReferVacancyAsync(long vacancyReference)
        {
            return messaging.SendCommandAsync(new ReferVacancyCommand
            {
                VacancyReference = vacancyReference
            });
        }

        public Task<IEnumerable<LegalEntity>> GetEmployerLegalEntitiesAsync(string employerAccountId)
        {
            return employerAccountProvider.GetEmployerLegalEntitiesAsync(employerAccountId);
        }

        public Task CreateApplicationReviewAsync(Domain.Entities.Application application)
        {
            return messaging.SendCommandAsync(new CreateApplicationReviewCommand { Application = application });
        }

        public Task WithdrawApplicationAsync(long vacancyReference, Guid candidateId)
        {
            return messaging.SendCommandAsync(new WithdrawApplicationCommand
            {
                VacancyReference = vacancyReference,
                CandidateId = candidateId
            });
        }

        public Task HardDeleteApplicationReviewsForCandidate(Guid candidateId)
        {
            return messaging.SendCommandAsync(new DeleteApplicationReviewsCommand
            {
                CandidateId = candidateId
            });
        }

        public Task PerformRulesCheckAsync(Guid reviewId)
        {
            return vacancyService.PerformRulesCheckAsync(reviewId);
        }

        public Task<VacancyReview> GetCurrentReferredVacancyReviewAsync(long vacancyReference)
        {
            return vacancyReviewQuery.GetCurrentReferredVacancyReviewAsync(vacancyReference);
        }

        public Task RefreshEmployerProfiles(string employerAccountId, IEnumerable<string> accountLegalEntityPublicHashedIds)
        {
            return messaging.SendCommandAsync(new RefreshEmployerProfilesCommand
            {
                EmployerAccountId = employerAccountId,
                AccountLegalEntityPublicHashedIds = accountLegalEntityPublicHashedIds
            });
        }
        public Task<User> GetUsersDetailsAsync(string userId)
        {
            return userRepository.GetAsync(userId);
        }

        public Task UpsertUserDetails(User user)
        {
            return userRepository.UpsertUserAsync(user);
        }
        
        public Task<User> GetUsersDetailsByDfEUserId(string dfeUserId)
        {
            return userRepository.GetByDfEUserId(dfeUserId);
        }

        public Task<VacancyAnalyticsSummary> GetVacancyAnalyticsSummaryAsync(long vacancyReference)
        {
            return reader.GetVacancyAnalyticsSummaryAsync(vacancyReference);
        }

        public async Task<UserNotificationPreferences> GetUserNotificationPreferencesAsync(string idamsUserId, string dfeUserId = null)
        {
            var preferences = await userNotificationPreferencesRepository.GetAsync(idamsUserId);

            if (dfeUserId != null)
            {
                return preferences;
            }
            
            return preferences ?? new UserNotificationPreferences { Id = idamsUserId};
        }
        
        public async Task<UserNotificationPreferences> GetUserNotificationPreferencesByDfEUserIdAsync(string idamsUserId, string dfeUserId = null)
        {
            var preferences = await userNotificationPreferencesRepository.GetByDfeUserId(dfeUserId) 
                              ?? await GetUserNotificationPreferencesAsync(idamsUserId, dfeUserId);

            return preferences ?? new UserNotificationPreferences() { Id = idamsUserId ,DfeUserId = dfeUserId};
        }

        public Task UpdateUserNotificationPreferencesAsync(UserNotificationPreferences preferences)
        {
            return messaging.SendCommandAsync(new UpdateUserNotificationPreferencesCommand
            {
                UserNotificationPreferences = preferences
            });
        }

        public EntityValidationResult ValidateUserNotificationPreferences(UserNotificationPreferences preferences)
        {
            var fluentResult = userNotificationPreferencesValidator.Validate(preferences);
            return EntityValidationResult.FromFluentValidationResult(fluentResult);
        }

        public Task UpdateUserAccountAsync(string idamsUserId)
        {
            return messaging.SendCommandAsync(new UpdateUserAccountCommand
            {
                IdamsUserId = idamsUserId
            });
        }

        public Task<int> GetVacancyCountForUserAsync(string userId)
        {
            return vacancyQuery.GetVacancyCountForUserAsync(userId);
        }

        public EntityValidationResult ValidateQualification(Qualification qualification)
        {
            ValidationResult fluentResult = qualificationValidator.Validate(qualification);
            return EntityValidationResult.FromFluentValidationResult(fluentResult);
        }
        
        public async Task<long> GetVacancyCount(string employerAccountId, FilteringOptions? filteringOptions, string searchTerm)
        {
            var ownerType = (filteringOptions == FilteringOptions.NewSharedApplications || filteringOptions == FilteringOptions.AllSharedApplications) ? OwnerType.Provider : OwnerType.Employer;
            return await vacancySummariesQuery.VacancyCount(null, employerAccountId, filteringOptions, searchTerm, ownerType);
        }
    }
}