using System;
using System.Collections.Generic;
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
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using FluentValidation;
using FluentValidation.Results;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public partial class VacancyClient : IRecruitVacancyClient, IEmployerVacancyClient, IJobsVacancyClient
    {
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
        private readonly IEmployerDashboardProjectionService _employerDashboardService;
        private readonly IProviderDashboardProjectionService _providerDashboardService;
        private readonly IEmployerProfileRepository _employerProfileRepository;
        private readonly IUserRepository _userRepository;
        private readonly IQualificationsProvider _qualificationsProvider;
        private readonly IEmployerService _employerService;
        private readonly IReportRepository _reportRepository;
        private readonly IReportService _reportService;
        private readonly IUserNotificationPreferencesRepository _userNotificationPreferencesRepository;
        private readonly AbstractValidator<UserNotificationPreferences> _userNotificationPreferencesValidator;
        private readonly AbstractValidator<Qualification> _qualificationValidator;

        public VacancyClient(
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
            IEmployerDashboardProjectionService employerDashboardService,
            IProviderDashboardProjectionService providerDashboardService,
            IEmployerProfileRepository employerProfileRepository,
            IUserRepository userRepository,
            IQualificationsProvider qualificationsProvider,
            IEmployerService employerService,
            IReportRepository reportRepository,
            IReportService reportService,
            IUserNotificationPreferencesRepository userNotificationPreferencesRepository,
            AbstractValidator<UserNotificationPreferences> userNotificationPreferencesValidator,
            AbstractValidator<Qualification> qualificationValidator)
        {
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
            _employerDashboardService = employerDashboardService;
            _providerDashboardService = providerDashboardService;
            _employerProfileRepository = employerProfileRepository;
            _userRepository = userRepository;
            _qualificationsProvider = qualificationsProvider;
            _employerService = employerService;
            _reportRepository = reportRepository;
            _reportService = reportService;
            _userNotificationPreferencesRepository = userNotificationPreferencesRepository;
            _userNotificationPreferencesValidator = userNotificationPreferencesValidator;
            _qualificationValidator = qualificationValidator;
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

        public async Task<EmployerDashboard> GetDashboardAsync(string employerAccountId, bool createIfNonExistent = false)
        {
            var dashboard = await _reader.GetEmployerDashboardAsync(employerAccountId);

            if (dashboard == null && createIfNonExistent)
            {
                await GenerateDashboard(employerAccountId);
                dashboard = await GetDashboardAsync(employerAccountId);
            }

            return dashboard;
        }

        public Task GenerateDashboard(string employerAccountId)
        {
            return _employerDashboardService.ReBuildDashboardAsync(employerAccountId);
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

        public Task<IApprenticeshipProgramme> GetApprenticeshipProgrammeAsync(string programmeId)
        {
            return _apprenticeshipProgrammesProvider.GetApprenticeshipProgrammeAsync(programmeId);
        }

        public Task<IEnumerable<string>> GetEmployerIdentifiersAsync(string userId)
        {
            return _employerAccountProvider.GetEmployerIdentifiersAsync(userId);
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

        public Task<VacancyApplications> GetVacancyApplicationsAsync(string vacancyReference)
        {
            return _reader.GetVacancyApplicationsAsync(vacancyReference);
        }

        public Task SetApplicationReviewSuccessful(Guid applicationReviewId, VacancyUser user)
        {
            var command = new ApplicationReviewSuccessfulCommand
            {
                ApplicationReviewId = applicationReviewId,
                User = user
            };

            return _messaging.SendCommandAsync(command);
        }

        public Task SetApplicationReviewUnsuccessful(Guid applicationReviewId, string candidateFeedback, VacancyUser user)
        {
            var command = new ApplicationReviewUnsuccessfulCommand
            {
                ApplicationReviewId = applicationReviewId,
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

        public Task UpdateApprenticeshipProgrammesAsync()
        {
            var command = new UpdateApprenticeshipProgrammesCommand();

            return _messaging.SendCommandAsync(command);
        }

        public Task UpdateApprenticeshipRouteAsync()
        {
            var command = new UpdateApprenticeshipRouteCommand();

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
            throw new NotImplementedException();
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

        public Task<VacancyAnalyticsSummary> GetVacancyAnalyticsSummaryAsync(long vacancyReference)
        {
            return _reader.GetVacancyAnalyticsSummaryAsync(vacancyReference);
        }

        public async Task<UserNotificationPreferences> GetUserNotificationPreferencesAsync(string idamsUserId)
        {
            var preferences = await _userNotificationPreferencesRepository.GetAsync(idamsUserId);

            return preferences ?? new UserNotificationPreferences() { Id = idamsUserId };
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
    }
}