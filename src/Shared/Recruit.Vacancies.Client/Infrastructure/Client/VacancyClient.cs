using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public class VacancyClient : IRecruitVacancyClient, IEmployerVacancyClient, IProviderVacancyClient, IJobsVacancyClient
    {
        private readonly IMessaging _messaging;
        private readonly IQueryStoreReader _reader;
        private readonly IVacancyRepository _repository;
        private readonly IEntityValidator<Vacancy, VacancyRuleSet> _validator;
        private readonly IApprenticeshipProgrammeProvider _apprenticeshipProgrammesProvider;
        private readonly IEmployerAccountProvider _employerAccountProvider;
        private readonly IReferenceDataReader _referenceDataReader;
        private readonly IApplicationReviewRepository _applicationReviewRepository;
        private readonly IVacancyReviewQuery _vacancyReviewQuery;
        private readonly ICandidateSkillsProvider _candidateSkillsProvider;
        private readonly IVacancyService _vacancyService;
        private readonly IEmployerDashboardProjectionService _employerDashboardService;
        private readonly IProviderDashboardProjectionService _providerDashboardService;

        private readonly IEmployerProfileRepository _employerProfileRepository;
        private readonly IUserRepository _userRepository;
        private readonly IQualificationsProvider _qualificationsProvider;

        public VacancyClient(
            IVacancyRepository repository,
            IQueryStoreReader reader,
            IMessaging messaging,
            IEntityValidator<Vacancy, VacancyRuleSet> validator,
            IApprenticeshipProgrammeProvider apprenticeshipProgrammesProvider,
            IEmployerAccountProvider employerAccountProvider,
            IReferenceDataReader referenceDataReader,
            IApplicationReviewRepository applicationReviewRepository,
            IVacancyReviewQuery vacancyReviewQuery,
            ICandidateSkillsProvider candidateSkillsProvider,
            IVacancyService vacancyService,
            IEmployerDashboardProjectionService employerDashboardService,
            IProviderDashboardProjectionService providerDashboardService,
            IEmployerProfileRepository employerProfileRepository,
            IUserRepository userRepository,
            IQualificationsProvider qualificationsProvider)
        {
            _repository = repository;
            _reader = reader;
            _messaging = messaging;
            _validator = validator;
            _apprenticeshipProgrammesProvider = apprenticeshipProgrammesProvider;
            _employerAccountProvider = employerAccountProvider;
            _referenceDataReader = referenceDataReader;
            _applicationReviewRepository = applicationReviewRepository;
            _vacancyReviewQuery = vacancyReviewQuery;
            _candidateSkillsProvider = candidateSkillsProvider;
            _vacancyService = vacancyService;
            _employerDashboardService = employerDashboardService;
            _providerDashboardService = providerDashboardService;
            _employerProfileRepository = employerProfileRepository;
            _userRepository = userRepository;
            _qualificationsProvider = qualificationsProvider;
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

        public Task UpdatePublishedVacancyAsync(Vacancy vacancy, VacancyUser user)
        {
            var command = new UpdateLiveVacancyCommand
            {
                Vacancy = vacancy,
                User = user
            };

            return _messaging.SendCommandAsync(command);
        }

        public Task<Vacancy> GetVacancyAsync(Guid vacancyId)
        {
            return _repository.GetVacancyAsync(vacancyId);
        }

        public async Task<Guid> CreateVacancyAsync(SourceOrigin origin, string title, int numberOfPositions, string employerAccountId, VacancyUser user, UserType userType)
        {
            var vacancyId = GenerateVacancyId();

            var command = new CreateEmployerOwnedVacancyCommand
            {
                VacancyId = vacancyId,
                User = user,
                UserType = userType,
                Title = title,
                NumberOfPositions = numberOfPositions,
                EmployerAccountId = employerAccountId,
                Origin = origin
            };

            await _messaging.SendCommandAsync(command);

            return vacancyId;
        }

        public async Task<Guid> CreateVacancyAsync(SourceOrigin origin, string title, int numberOfPositions, long ukprn, VacancyUser user, UserType userType)
        {
            var vacancyId = GenerateVacancyId();

            var command = new CreateProviderOwnedVacancyCommand
            {
                VacancyId = vacancyId,
                User = user,
                UserType = userType,
                Title = title,
                NumberOfPositions = numberOfPositions,
                //EmployerAccountId = employerAccountId, // need training provider
                Origin = origin
            };

            await _messaging.SendCommandAsync(command);

            return vacancyId;
        }

        public async Task<Guid> CloneVacancyAsync(Guid vacancyId, VacancyUser user, SourceOrigin sourceOrigin)
        {
            var newVacancyId = GenerateVacancyId();

            var command = new CloneVacancyCommand(cloneVacancyId: vacancyId, newVacancyId: newVacancyId, user: user, sourceOrigin: sourceOrigin);

            await _messaging.SendCommandAsync(command);

            return newVacancyId;
        }
        
        private Guid GenerateVacancyId()
        {
            return Guid.NewGuid();
        }

        public Task SubmitVacancyAsync(Guid vacancyId, string employerDescription, VacancyUser user)
        {
            var command = new SubmitVacancyCommand
            {
                VacancyId = vacancyId,
                EmployerDescription = employerDescription,
                User = user
            };

            return _messaging.SendCommandAsync(command);
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

        public Task<EmployerDashboard> GetDashboardAsync(string employerAccountId)
        {
            return _reader.GetEmployerDashboardAsync(employerAccountId);
        }

        public Task GenerateDashboard(string employerAccountId)
        {
            return _employerDashboardService.ReBuildDashboardAsync(employerAccountId);
        }

        public Task<ProviderDashboard> GetDashboardAsync(long ukprn)
        {
            return _reader.GetProviderDashboardAsync(ukprn);
        }

        public Task GenerateDashboard(long ukprn)
        {
            return _providerDashboardService.ReBuildDashboardAsync(ukprn);
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

        public Task<EditVacancyInfo> GetEditVacancyInfoAsync(string employerAccountId)
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

        public Task<EmployerProfile> GetEmployerProfileAsync(string employerAccountId, long legalEntityId)
        {
            return _employerProfileRepository.GetAsync(employerAccountId, legalEntityId);
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

        public Task UpdateApprenticeshipProgrammesAsync()
        {
            var command = new UpdateApprenticeshipProgrammesCommand();

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

        public Task<IEnumerable<LiveVacancy>> GetLiveVacancies()
        {
            return _reader.GetLiveVacancies();
        }

        public async Task CloseVacancyAsync(Guid vacancyId, VacancyUser user)
        {
            var command = new CloseVacancyCommand
            {
                VacancyId = vacancyId,
                User = user
            };

            await _messaging.SendCommandAsync(command);
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

        public async Task ApproveVacancy(long vacancyReference)
        {
            await _messaging.SendCommandAsync(new ApproveVacancyCommand
            {
                VacancyReference = vacancyReference
            });
        }

        public Task ReferVacancy(long vacancyReference)
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

        public Task RefreshEmployerProfiles(string employerAccountId, IEnumerable<long> legalEntityIds)
        {
            return _messaging.SendCommandAsync(new RefreshEmployerProfilesCommand
            {
                EmployerAccountId = employerAccountId,
                LegalEntityIds = legalEntityIds
            });
        }
        public Task<User> GetUsersDetailsAsync(string userId)
        {
            return _userRepository.GetAsync(userId);
        }

        public Task SaveLevyDeclarationAsync(string userId, string employerAccountId)
        {
            return _messaging.SendCommandAsync(new SaveUserLevyDeclarationCommand
            {
                UserId = userId,
                EmployerAccountId = employerAccountId
            });
        }
    }
}