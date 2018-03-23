using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.QueryStore;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Services;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public class VacancyClient : IVacancyClient
    {
        private readonly IMessaging _messaging;
        private readonly IQueryStoreReader _reader;
        private readonly IVacancyRepository _repository;
        private readonly ITimeProvider _timeProvider;
        private readonly IEntityValidator<Vacancy, VacancyRuleSet> _validator;

        public VacancyClient(IVacancyRepository repository, IQueryStoreReader reader, IMessaging messaging, ITimeProvider timeProvider, IEntityValidator<Vacancy, VacancyRuleSet> validator)
        {
            _timeProvider = timeProvider;
            _repository = repository;
            _reader = reader;
            _messaging = messaging;
            _validator = validator;
        }

        public async Task UpdateVacancyAsync(Vacancy vacancy, VacancyRuleSet validationRules, bool canUpdateQueryStore = true)
        {
            var command = new UpdateVacancyCommand
            {
                Vacancy = vacancy,
                ValidationRules = validationRules
            };

            await _messaging.SendCommandAsync(command);

            if (canUpdateQueryStore)
                await UpdateDashboardAsync(vacancy.EmployerAccountId);
        }

        public async Task<Vacancy> GetVacancyForEditAsync(Guid id)
        {
            return await _repository.GetVacancyAsync(id);
        }

        public async Task<Guid> CreateVacancyAsync(string title, string employerAccountId, string user)
        {
            var command = new CreateVacancyCommand
            {
                Vacancy = new Vacancy
                {
                    Id = Guid.NewGuid(),
                    Title = title,
                    EmployerAccountId = employerAccountId,
                    Status = VacancyStatus.Draft,
                    CreatedDate = _timeProvider.Now,
                    CreatedBy = user,
                    IsDeleted = false
                }
            };

            await _messaging.SendCommandAsync(command);
            await UpdateDashboardAsync(employerAccountId);

            return command.Vacancy.Id;
        }

        public async Task<bool> SubmitVacancyAsync(Guid id)
        {
            var vacancy = await GetVacancyForEditAsync(id);

            if(!vacancy.CanSubmit)
            {
                return false;
            }

            vacancy.Status = VacancyStatus.Submitted;
            vacancy.SubmittedDate = _timeProvider.Now;

            var command = new SubmitVacancyCommand
            {
                Vacancy = vacancy
            };

            await _messaging.SendCommandAsync(command);
            await UpdateDashboardAsync(vacancy.EmployerAccountId);

            return true;
        }

        public async Task<bool> DeleteVacancyAsync(Guid id)
        {
            var vacancy = await GetVacancyForEditAsync(id);
            
            if (vacancy == null || vacancy.CanDelete == false)
            {
                return false;
            }

            vacancy.IsDeleted = true;
            vacancy.DeletedDate = _timeProvider.Now;

            var command = new DeleteVacancyCommand
            {
                Vacancy = vacancy
            };

            await _messaging.SendCommandAsync(command);
            await UpdateDashboardAsync(vacancy.EmployerAccountId);

            return true;
        }
        
        public async Task<Dashboard> GetDashboardAsync(string employerAccountId)
        {
            return await _reader.GetDashboardAsync(employerAccountId);
        }

        public async Task UpdateApprenticeshipProgrammesAsync(IEnumerable<ApprenticeshipProgramme> programmes)
        {
            var command = new UpdateApprenticeshipProgrammesCommand
            {
                ApprenticeshipProgrammes = programmes
            };

            await _messaging.SendCommandAsync(command);
        }

        public async Task<ApprenticeshipProgrammes> GetApprenticeshipProgrammesAsync()
        {
            return await _reader.GetApprenticeshipProgrammesAsync();
        }

        private async Task UpdateDashboardAsync(string employerAccountId)
        {
            var command = new UpdateDashboardCommand
            {
                EmployerAccountId = employerAccountId,
            };

            await _messaging.SendCommandAsync(command);
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
    }
}