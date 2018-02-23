using System;
using System.Threading.Tasks;
using Esfa.Recruit.Storage.Client.Application.Commands;
using Esfa.Recruit.Storage.Client.Core.Commands;
using Esfa.Recruit.Storage.Client.Domain.Entities;
using Esfa.Recruit.Storage.Client.Domain.Enum;
using Esfa.Recruit.Storage.Client.Domain.Messaging;
using Esfa.Recruit.Storage.Client.Domain.QueryStore;
using Esfa.Recruit.Storage.Client.Domain.Repositories;

namespace Recruit.Vacancies.Client.Infrastructure.Client
{
    public class VacancyClient : IVacancyClient
    {
        private readonly IMessaging _messaging;
        private readonly IQueryStoreReader _queryStore;
        private readonly IVacancyRepository _repository;

        public VacancyClient(IVacancyRepository repository, IQueryStoreReader queryStore, IMessaging messaging)
        {
            _repository = repository;
            _queryStore = queryStore;
            _messaging = messaging;
        }

        public Task UpdateVacancyAsync(Vacancy vacancy)
        {
            var command = new UpdateVacancyCommand
            {
                Vacancy = vacancy
            };

            return _messaging.SendCommandAsync(command);
        }

        public async Task<Vacancy> GetVacancyForEditAsync(Guid id)
        {
            return await _repository.GetVacancyAsync(id);
        }

        public async Task<Guid> CreateVacancyAsync(string title, string employerAccountId)
        {
            var command = new CreateVacancyCommand
            {
                Vacancy = new Vacancy
                {
                    Id = Guid.NewGuid(),
                    Title = title,
                    EmployerAccountId = employerAccountId,
                    Status = VacancyStatus.Draft,
                    CreatedDate = DateTime.Now
                }
            };

            await _messaging.SendCommandAsync(command);

            return command.Vacancy.Id;
        }

        public async Task<bool> SubmitVacancyAsync(Guid id)
        {

            var vacancy = await GetVacancyForEditAsync(id);

            if(!vacancy.IsSubmittable())
            {
                return false;
            }

            vacancy.Status = VacancyStatus.Submitted;
            vacancy.SubmittedDate = DateTime.Now;

            var command = new SubmitVacancyCommand
            {
                Vacancy = vacancy
            };
                
            await _messaging.SendCommandAsync(command);

            return true;
        }
    }
}