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
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mappings;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Dashboard;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Models;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public class VacancyClient : IEmployerVacancyClient, IJobsVacancyClient
    {
        private readonly IMessaging _messaging;
        private readonly IQueryStoreReader _reader;
        private readonly IQueryStoreWriter _writer;
        private readonly IVacancyRepository _repository;
        private readonly ITimeProvider _timeProvider;
        private readonly IEntityValidator<Vacancy, VacancyRuleSet> _validator;
        private readonly IApprenticeshipProgrammeProvider _apprenticeshipProgrammesProvider;
        private readonly IEmployerAccountService _employerAccountService;

        public VacancyClient(
            IVacancyRepository repository, 
            IQueryStoreReader reader, 
            IQueryStoreWriter writer, 
            IMessaging messaging, 
            ITimeProvider timeProvider, 
            IEntityValidator<Vacancy, VacancyRuleSet> validator, 
            IApprenticeshipProgrammeProvider apprenticeshipProgrammesProvider,
            IEmployerAccountService employerAccountService)
        {
            _timeProvider = timeProvider;
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
            vacancy.LastUpdatedDate = _timeProvider.Now;
            vacancy.LastUpdatedByUser = user;
            
            var command = new UpdateVacancyCommand
            {
                Vacancy = vacancy
            };

            return _messaging.SendCommandAsync(command);
        }

        public Task<Vacancy> GetVacancyAsync(Guid id)
        {
            return _repository.GetVacancyAsync(id);
        }

        public async Task<Guid> CreateVacancyAsync(SourceOrigin origin, string title, string employerAccountId, VacancyUser user)
        {
            var now = _timeProvider.Now;

            var command = new CreateVacancyCommand
            {
                Vacancy = new Vacancy
                {
                    Id = Guid.NewGuid(),
                    SourceOrigin = origin,
                    SourceType = SourceType.New,
                    Title = title,
                    EmployerAccountId = employerAccountId,
                    Status = VacancyStatus.Draft,
                    CreatedDate = now,
                    CreatedByUser = user,
                    LastUpdatedDate = now,
                    LastUpdatedByUser = user,
                    IsDeleted = false
                }
            };

            await _messaging.SendCommandAsync(command);

            return command.Vacancy.Id;
        }

        public async Task<bool> SubmitVacancyAsync(Guid id, VacancyUser user)
        {
            var vacancy = await GetVacancyAsync(id);

            if(!vacancy.CanSubmit)
            {
                return false;
            }

            var now = _timeProvider.Now;

            vacancy.Status = VacancyStatus.Submitted;
            vacancy.SubmittedDate = now;
            vacancy.SubmittedByUser = user;
            vacancy.LastUpdatedDate = now;
            vacancy.LastUpdatedByUser = user;
            
            var command = new SubmitVacancyCommand
            {
                Vacancy = vacancy
            };

            await _messaging.SendCommandAsync(command);
            
            return true;
        }

        public async Task<bool> ApproveVacancyAsync(Guid id)
        {
            var command = new ApproveVacancyCommand()
            {
                VacancyId = id
            };

            await _messaging.SendCommandAsync(command);

            return true;
        }

        public async Task<bool> DeleteVacancyAsync(Guid id, VacancyUser user)
        {
            var vacancy = await GetVacancyAsync(id);
            
            if (vacancy == null || vacancy.CanDelete == false)
            {
                return false;
            }

            var now = _timeProvider.Now;

            vacancy.IsDeleted = true;
            vacancy.DeletedDate = now;
            vacancy.DeletedByUser = user;
            vacancy.LastUpdatedDate = now;
            vacancy.LastUpdatedByUser = user;

            var command = new DeleteVacancyCommand
            {
                Vacancy = vacancy
            };

            await _messaging.SendCommandAsync(command);

            return true;
        }
        
        public Task<Dashboard> GetDashboardAsync(string employerAccountId)
        {
            return  _reader.GetDashboardAsync(employerAccountId);
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
        public Task AssignVacancyNumber(Guid id)
        {
            var command = new AssignVacancyNumberCommand
            {
                VacancyId = id
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

        // Shared
        public async Task<IEnumerable<LegalEntity>> GetEmployerLegalEntitiesAsync(string employerAccountId)
        {
            var results = await _employerAccountService.GetEmployerLegalEntitiesAsync(employerAccountId);

            return results.Select(LegalEntityMapper.MapFromAccountApiLegalEntity);
        }
    }
}