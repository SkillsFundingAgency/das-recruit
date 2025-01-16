using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services.Reports;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public partial class VacancyClient : IRecruitVacancyClient, IEmployerVacancyClient, IJobsVacancyClient
    {
        private readonly ILogger<VacancyClient> _logger;
        private readonly IMessaging _messaging;
        private readonly IQueryStoreReader _reader;
        private readonly IVacancyRepository _repository;
        private readonly IVacancyQuery _vacancyQuery;
        private readonly IEntityValidator<Vacancy, VacancyRuleSet> _validator;
        private readonly IApprenticeshipProgrammeProvider _apprenticeshipProgrammesProvider;
        private readonly IEmployerAccountProvider _employerAccountProvider;
        private readonly IApplicationReviewRepository _applicationReviewRepository;
        private readonly IVacancyReviewQuery _vacancyReviewQuery;
        private readonly ICandidateSkillsProvider _candidateSkillsProvider;
        private readonly IVacancyService _vacancyService;
        private readonly IEmployerProfileRepository _employerProfileRepository;
        private readonly IUserRepository _userRepository;
        private readonly IQualificationsProvider _qualificationsProvider;
        private readonly IEmployerService _employerService;
        private readonly IReportRepository _reportRepository;
        private readonly IReportService _reportService;
        private readonly IUserNotificationPreferencesRepository _userNotificationPreferencesRepository;
        private readonly AbstractValidator<UserNotificationPreferences> _userNotificationPreferencesValidator;
        private readonly AbstractValidator<Qualification> _qualificationValidator;
        private readonly IApprenticeshipRouteProvider _apprenticeshipRouteProvider;
        private readonly IVacancySummariesProvider _vacancySummariesQuery;
        private readonly ITimeProvider _timeProvider;

        public VacancyClient(
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
            IApprenticeshipRouteProvider apprenticeshipRouteProvider, 
            IVacancySummariesProvider vacancySummariesQuery, 
            ITimeProvider timeProvider)
        {
            _logger = logger;
            _repository = repository;
            _vacancyQuery = vacancyQuery;
            _reader = reader;
            _messaging = messaging;
            _validator = validator;
            _apprenticeshipProgrammesProvider = apprenticeshipProgrammesProvider;
            _employerAccountProvider = employerAccountProvider;
            _applicationReviewRepository = applicationReviewRepository;
            _vacancyReviewQuery = vacancyReviewQuery;
            _candidateSkillsProvider = candidateSkillsProvider;
            _vacancyService = vacancyService;
            _employerProfileRepository = employerProfileRepository;
            _userRepository = userRepository;
            _qualificationsProvider = qualificationsProvider;
            _employerService = employerService;
            _reportRepository = reportRepository;
            _reportService = reportService;
            _userNotificationPreferencesRepository = userNotificationPreferencesRepository;
            _userNotificationPreferencesValidator = userNotificationPreferencesValidator;
            _qualificationValidator = qualificationValidator;
            _apprenticeshipRouteProvider = apprenticeshipRouteProvider;
            _vacancySummariesQuery = vacancySummariesQuery;
            _timeProvider = timeProvider;
        }

        public Task UpdateDraftVacancyAsync(Vacancy vacancy, VacancyUser user)
        {
            var command = new UpdateDraftVacancyCommand
            {
                Vacancy = vacancy,
                User = user
            };

            return _messaging.SendCommandAsync(command);
        }

        public Task UpdatePublishedVacancyAsync(Vacancy vacancy, VacancyUser user, LiveUpdateKind updateKind)
        {
            var command = new UpdateLiveVacancyCommand(vacancy, user, updateKind);

            return _messaging.SendCommandAsync(command);
        }

        public Task<Vacancy> GetVacancyAsync(Guid vacancyId)
        {
            return _repository.GetVacancyAsync(vacancyId);
        }

        public Task<string> GetEmployerNameAsync(Vacancy vacancy)
        {
            return _employerService.GetEmployerNameAsync(vacancy);
        }

        public Task<string> GetEmployerDescriptionAsync(Vacancy vacancy)
        {
            return _employerService.GetEmployerDescriptionAsync(vacancy);
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

            await _messaging.SendCommandAsync(command);

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
            
            await _messaging.SendCommandAsync(command);
            
            await AssignVacancyNumber(id);
        }

        public async Task<Guid> CloneVacancyAsync(
            Guid vacancyId, VacancyUser user, SourceOrigin sourceOrigin,
            DateTime startDate, DateTime closingDate)
        {
            var newVacancyId = GenerateVacancyId();

            var command = new CloneVacancyCommand(vacancyId, newVacancyId, user, sourceOrigin, startDate, closingDate);

            await _messaging.SendCommandAsync(command);

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

            return _messaging.SendCommandAsync(command);
        }
        
        public async Task<EmployerDashboardSummary> GetDashboardSummary(string employerAccountId)
        {
            var dashboardValue = await  _vacancySummariesQuery.GetEmployerOwnedVacancyDashboardByEmployerAccountIdAsync(employerAccountId);
            
            var dashboard = dashboardValue.VacancyStatusDashboard;
            var dashboardApplications = dashboardValue.VacancyApplicationsDashboard;
            var dashboardSharedApplications = dashboardValue.VacancySharedApplicationsDashboard;

            return new EmployerDashboardSummary
            {
                Closed = dashboard.FirstOrDefault(c=>c.Status == VacancyStatus.Closed)?.StatusCount ?? 0,
                Draft = dashboard.SingleOrDefault(c=>c.Status == VacancyStatus.Draft)?.StatusCount ?? 0,
                Review = dashboard.SingleOrDefault(c=>c.Status == VacancyStatus.Review)?.StatusCount ?? 0,
                Referred = (dashboard.SingleOrDefault(c=>c.Status == VacancyStatus.Referred)?.StatusCount ?? 0) + (dashboard.SingleOrDefault(c=>c.Status == VacancyStatus.Rejected)?.StatusCount ?? 0),
                Live = dashboard.Where(c=>c.Status == VacancyStatus.Live).Sum(c=>c.StatusCount),
                Submitted = dashboard.SingleOrDefault(c=>c.Status == VacancyStatus.Submitted)?.StatusCount ?? 0,
                NumberOfNewApplications = dashboardApplications.Where(c=>c.Status == VacancyStatus.Live || c.Status == VacancyStatus.Closed).Sum(x=>x.NoOfNewApplications),
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
                await _vacancySummariesQuery.GetEmployerOwnedVacancySummariesByEmployerAccountId(employerAccountId,
                     page, status, searchTerm);
            return new EmployerDashboard
            {
                Id = QueryViewType.EmployerDashboard.GetIdValue(employerAccountId),
                Vacancies = vacancySummaries,
                LastUpdated = _timeProvider.Now
            };
        }

        public Task UserSignedInAsync(VacancyUser user, UserType userType)
        {
            var command = new UserSignedInCommand(user, userType);

            return _messaging.SendCommandAsync(command);
        }

        public Task SetupEmployerAsync(string employerAccountId)
        {
            var command = new SetupEmployerCommand
            {
                EmployerAccountId = employerAccountId
            };

            return _messaging.SendCommandAsync(command);
        }

        public Task<EmployerEditVacancyInfo> GetEditVacancyInfoAsync(string employerAccountId)
        {
            return _reader.GetEmployerVacancyDataAsync(employerAccountId);
        }

        public EntityValidationResult Validate(Vacancy vacancy, VacancyRuleSet rules)
        {
            return _validator.Validate(vacancy, rules);
        }

        public Task<IEnumerable<IApprenticeshipProgramme>> GetActiveApprenticeshipProgrammesAsync()
        {
            return _apprenticeshipProgrammesProvider.GetApprenticeshipProgrammesAsync();
        }

        public Task<IEnumerable<IApprenticeshipRoute>> GetApprenticeshipRoutes()
        {
            return _apprenticeshipRouteProvider.GetApprenticeshipRoutesAsync();
        }

        public Task<IApprenticeshipProgramme> GetApprenticeshipProgrammeAsync(string programmeId)
        {
            return _apprenticeshipProgrammesProvider.GetApprenticeshipProgrammeAsync(programmeId);
        }

        public Task<GetUserAccountsResponse> GetEmployerIdentifiersAsync(string userId, string email)
        {
            return _employerAccountProvider.GetEmployerIdentifiersAsync(userId, email);
        }

        public Task<List<string>> GetCandidateSkillsAsync()
        {
            return _candidateSkillsProvider.GetCandidateSkillsAsync();
        }

        public Task<IList<string>> GetCandidateQualificationsAsync()
        {
            return _qualificationsProvider.GetQualificationsAsync();
        }

        public Task<ApplicationReview> GetApplicationReviewAsync(Guid applicationReviewId)
        {
            return _applicationReviewRepository.GetAsync(applicationReviewId);
        }

        public async Task<List<VacancyApplication>> GetVacancyApplicationsSortedAsync(long vacancyReference, SortColumn sortColumn, SortOrder sortOrder, bool vacancySharedByProvider = false)
        {
            var applicationReviews = vacancySharedByProvider
                ? await _applicationReviewRepository.GetForSharedVacancySortedAsync(vacancyReference, sortColumn, sortOrder)
                : await _applicationReviewRepository.GetForVacancySortedAsync(vacancyReference, sortColumn, sortOrder);

            return applicationReviews == null
                ? new List<VacancyApplication>()
                : applicationReviews.Select(c => (VacancyApplication)c).ToList();
        }

        public async Task<List<VacancyApplication>> GetVacancyApplicationsAsync(long vacancyReference, bool vacancySharedByProvider = false)
        {
            var applicationReviews = vacancySharedByProvider
                ? await _applicationReviewRepository.GetForSharedVacancyAsync(vacancyReference) 
                : await _applicationReviewRepository.GetForVacancyAsync<ApplicationReview>(vacancyReference);

            return applicationReviews == null 
                ? new List<VacancyApplication>() 
                : applicationReviews.Select(c=>(VacancyApplication)c).ToList();
        }

        public async Task<List<VacancyApplication>> GetVacancyApplicationsForSelectedIdsAsync(List<Guid> applicationReviewIds)
        {
            var applicationReviews =
                await _applicationReviewRepository.GetAllForSelectedIdsAsync<ApplicationReview>(applicationReviewIds);

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

            return _messaging.SendStatusCommandAsync(command);
        }

        public Task SetApplicationReviewsShared(IEnumerable<VacancyApplication> applicationReviews, VacancyUser user)
        {
            var command = new ApplicationReviewsSharedCommand
            {
                ApplicationReviews = applicationReviews,
                User = user
            };

            return _messaging.SendCommandAsync(command);
        }

        public Task SetApplicationReviewsToUnsuccessful(IEnumerable<VacancyApplication> applicationReviewsToUnsuccessful, string candidateFeedback, VacancyUser user)
        {
            var command = new ApplicationReviewsUnsuccessfulCommand()
            {
                ApplicationReviews = applicationReviewsToUnsuccessful,
                CandidateFeedback = candidateFeedback,
                User = user
            };

            return _messaging.SendCommandAsync(command);
        }

        public Task<EmployerProfile> GetEmployerProfileAsync(string employerAccountId, string accountLegalEntityPublicHashedId)
        {
            return _employerProfileRepository.GetAsync(employerAccountId, accountLegalEntityPublicHashedId);
        }

        public Task UpdateEmployerProfileAsync(EmployerProfile employerProfile, VacancyUser user)
        {
            var command = new UpdateEmployerProfileCommand
            {
                Profile = employerProfile,
                User = user
            };

            return _messaging.SendCommandAsync(command);
        }

        // Jobs
        public Task AssignVacancyNumber(Guid vacancyId)
        {
            var command = new AssignVacancyNumberCommand
            {
                VacancyId = vacancyId
            };

            return _messaging.SendCommandAsync(command);
        }

        public Task PatchTrainingProviderAsync(Guid vacancyId)
        {
            var command = new PatchVacancyTrainingProviderCommand(vacancyId);

            return _messaging.SendCommandAsync(command);
        }

        public Task UpdateProviders()
        {
            var command = new UpdateProvidersCommand();

            return _messaging.SendCommandAsync(command);
        }


        public Task UpdateBankHolidaysAsync()
        {
            var command = new UpdateBankHolidaysCommand();

            return _messaging.SendCommandAsync(command);
        }

        public async Task CreateVacancyReview(long vacancyReference)
        {
            var command = new CreateVacancyReviewCommand
            {
                VacancyReference = vacancyReference
            };

            await _messaging.SendCommandAsync(command);
        }

        public Task CloseVacancyAsync(Guid vacancyId, VacancyUser user, ClosureReason reason)
        {
            return _messaging.SendCommandAsync(new CloseVacancyCommand(vacancyId, user, reason));
        }

        public Task<IApprenticeshipRoute> GetRoute(int? routeId)
        {
            return _apprenticeshipRouteProvider.GetApprenticeshipRouteAsync(routeId.GetValueOrDefault());
        }

        public async Task CloseExpiredVacancies()
        {
            var command = new CloseExpiredVacanciesCommand();

            await _messaging.SendCommandAsync(command);
        }

        public async Task EnsureVacancyIsGeocodedAsync(Guid vacancyId)
        {
            var vacancy = await _repository.GetVacancyAsync(vacancyId);

            if (!string.IsNullOrEmpty(vacancy?.EmployerLocation?.Postcode) &&
                vacancy.EmployerLocation?.HasGeocode == false)
            {
                await GeocodeVacancyAsync(vacancy.Id);
            }
        }

        private async Task GeocodeVacancyAsync(Guid vacancyId)
        {
            var command = new GeocodeVacancyCommand()
            {
                VacancyId = vacancyId
            };

            await _messaging.SendCommandAsync(command);
        }

        public Task ReferVacancyAsync(long vacancyReference)
        {
            return _messaging.SendCommandAsync(new ReferVacancyCommand
            {
                VacancyReference = vacancyReference
            });
        }

        public Task<IEnumerable<LegalEntity>> GetEmployerLegalEntitiesAsync(string employerAccountId)
        {
            return _employerAccountProvider.GetEmployerLegalEntitiesAsync(employerAccountId);
        }

        public Task CreateApplicationReviewAsync(Domain.Entities.Application application)
        {
            return _messaging.SendCommandAsync(new CreateApplicationReviewCommand { Application = application });
        }

        public Task WithdrawApplicationAsync(long vacancyReference, Guid candidateId)
        {
            return _messaging.SendCommandAsync(new WithdrawApplicationCommand
            {
                VacancyReference = vacancyReference,
                CandidateId = candidateId
            });
        }

        public Task HardDeleteApplicationReviewsForCandidate(Guid candidateId)
        {
            return _messaging.SendCommandAsync(new DeleteApplicationReviewsCommand
            {
                CandidateId = candidateId
            });
        }

        public Task PerformRulesCheckAsync(Guid reviewId)
        {
            return _vacancyService.PerformRulesCheckAsync(reviewId);
        }

        public Task<VacancyReview> GetCurrentReferredVacancyReviewAsync(long vacancyReference)
        {
            return _vacancyReviewQuery.GetCurrentReferredVacancyReviewAsync(vacancyReference);
        }

        public Task RefreshEmployerProfiles(string employerAccountId, IEnumerable<string> accountLegalEntityPublicHashedIds)
        {
            return _messaging.SendCommandAsync(new RefreshEmployerProfilesCommand
            {
                EmployerAccountId = employerAccountId,
                AccountLegalEntityPublicHashedIds = accountLegalEntityPublicHashedIds
            });
        }
        public Task<User> GetUsersDetailsAsync(string userId)
        {
            return _userRepository.GetAsync(userId);
        }

        public Task UpsertUserDetails(User user)
        {
            return _userRepository.UpsertUserAsync(user);
        }
        
        public Task<User> GetUsersDetailsByDfEUserId(string dfeUserId)
        {
            return _userRepository.GetByDfEUserId(dfeUserId);
        }

        public Task<VacancyAnalyticsSummary> GetVacancyAnalyticsSummaryAsync(long vacancyReference)
        {
            return _reader.GetVacancyAnalyticsSummaryAsync(vacancyReference);
        }

        public async Task<UserNotificationPreferences> GetUserNotificationPreferencesAsync(string idamsUserId, string dfeUserId = null)
        {
            var preferences = await _userNotificationPreferencesRepository.GetAsync(idamsUserId);

            if (dfeUserId != null)
            {
                return preferences;
            }
            
            return preferences ?? new UserNotificationPreferences { Id = idamsUserId};
        }
        
        public async Task<UserNotificationPreferences> GetUserNotificationPreferencesByDfEUserIdAsync(string idamsUserId, string dfeUserId = null)
        {
            var preferences = await _userNotificationPreferencesRepository.GetByDfeUserId(dfeUserId) 
                              ?? await GetUserNotificationPreferencesAsync(idamsUserId, dfeUserId);

            return preferences ?? new UserNotificationPreferences() { Id = idamsUserId ,DfeUserId = dfeUserId};
        }

        public Task UpdateUserNotificationPreferencesAsync(UserNotificationPreferences preferences)
        {
            return _messaging.SendCommandAsync(new UpdateUserNotificationPreferencesCommand
            {
                UserNotificationPreferences = preferences
            });
        }

        public EntityValidationResult ValidateUserNotificationPreferences(UserNotificationPreferences preferences)
        {
            var fluentResult = _userNotificationPreferencesValidator.Validate(preferences);
            return EntityValidationResult.FromFluentValidationResult(fluentResult);
        }

        public Task UpdateUserAccountAsync(string idamsUserId)
        {
            return _messaging.SendCommandAsync(new UpdateUserAccountCommand
            {
                IdamsUserId = idamsUserId
            });
        }

        public Task<int> GetVacancyCountForUserAsync(string userId)
        {
            return _vacancyQuery.GetVacancyCountForUserAsync(userId);
        }

        public EntityValidationResult ValidateQualification(Qualification qualification)
        {
            ValidationResult fluentResult = _qualificationValidator.Validate(qualification);
            return EntityValidationResult.FromFluentValidationResult(fluentResult);
        }
        
        public async Task<long> GetVacancyCount(string employerAccountId, FilteringOptions? filteringOptions, string searchTerm)
        {
            var ownerType = (filteringOptions == FilteringOptions.NewSharedApplications || filteringOptions == FilteringOptions.AllSharedApplications) ? OwnerType.Provider : OwnerType.Employer;
            return await _vacancySummariesQuery.VacancyCount(null, employerAccountId, filteringOptions, searchTerm, ownerType);
        }
    }
}