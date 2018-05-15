using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services.Models;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mappings;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Dashboard;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.LiveVacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Models;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public class VacancyClient : IEmployerVacancyClient, IJobsVacancyClient
    {
        private readonly IMessaging _messaging;
        private readonly IQueryStoreReader _reader;
        private readonly IQueryStoreWriter _writer;
        private readonly IVacancyRepository _repository;
        private readonly IEntityValidator<Vacancy, VacancyRuleSet> _validator;
        private readonly IApprenticeshipProgrammeProvider _apprenticeshipProgrammesProvider;
        private readonly IEmployerAccountService _employerAccountService;

        public VacancyClient(
            IVacancyRepository repository,
            IQueryStoreReader reader,
            IQueryStoreWriter writer,
            IMessaging messaging,
            IEntityValidator<Vacancy, VacancyRuleSet> validator,
            IApprenticeshipProgrammeProvider apprenticeshipProgrammesProvider,
            IEmployerAccountService employerAccountService)
        {
            _repository = repository;
            _reader = reader;
            _writer = writer;
            _messaging = messaging;
            _validator = validator;
            _apprenticeshipProgrammesProvider = apprenticeshipProgrammesProvider;
            _employerAccountService = employerAccountService;
        }

        public Task UpdateVacancyAsync(Vacancy vacancy, VacancyUser user)
        {
            var command = new UpdateVacancyCommand
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

        public async Task<Guid> CreateVacancyAsync(SourceOrigin origin, string title, string employerAccountId, VacancyUser user)
        {
            var vacancyId = Guid.NewGuid();

            var command = new CreateVacancyCommand
            {
                VacancyId = vacancyId,
                User = user,
                Title = title,
                EmployerAccountId = employerAccountId,
                Origin = origin
            };

            await _messaging.SendCommandAsync(command);

            return vacancyId;
        }

        public Task SubmitVacancyAsync(Guid vacancyId, VacancyUser user)
        {
            var command = new SubmitVacancyCommand
            {
                VacancyId = vacancyId,
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

        public Task<Dashboard> GetDashboardAsync(string employerAccountId)
        {
            return _reader.GetDashboardAsync(employerAccountId);
        }

        public Task RecordEmployerAccountSignInAsync(string employerAccountId)
        {
            var command = new UpdateUserCommand
            {
                EmployerAccountId = employerAccountId
            };

            return _messaging.SendCommandAsync(command);
        }

        public Task<EditVacancyInfo> GetEditVacancyInfo(string employerAccountId)
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
            return _employerAccountService.GetEmployerIdentifiersAsync(userId);
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

        public Task UpdateApprenticeshipProgrammesAsync(IEnumerable<ApprenticeshipProgramme> programmes)
        {
            return _writer.UpdateApprenticeshipProgrammesAsync(programmes);
        }

        public Task UpdateEmployerVacancyDataAsync(string employerAccountId, IEnumerable<LegalEntity> legalEntities)
        {
            return _writer.UpdateEmployerVacancyDataAsync(employerAccountId, legalEntities);
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

        public async Task CloseVacancy(Guid vacancyId)
        {
            var command = new CloseVacancyCommand
            {
                VacancyId = vacancyId
            };

            await _messaging.SendCommandAsync(command);
        }

        public async Task EnsureVacancyIsGeocodedAsync(Guid vacancyId)
        {
            var vacancy = await _repository.GetVacancyAsync(vacancyId);

            if (!string.IsNullOrEmpty(vacancy?.EmployerLocation?.Postcode) &&
                vacancy?.EmployerLocation?.HasGeocode == false)
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

        // Shared
        public async Task<IEnumerable<LegalEntity>> GetEmployerLegalEntitiesAsync(string employerAccountId)
        {
            var results = await _employerAccountService.GetEmployerLegalEntitiesAsync(employerAccountId);

            return results.Select(LegalEntityMapper.MapFromAccountApiLegalEntity);
        }
    }
}