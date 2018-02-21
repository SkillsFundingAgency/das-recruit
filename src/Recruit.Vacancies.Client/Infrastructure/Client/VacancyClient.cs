using System;
using System.Threading.Tasks;
using Esfa.Recruit.Storage.Client.Application.Commands;
using Esfa.Recruit.Storage.Client.Domain.Entities;
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

        public Task CreateVacancyAsync(Vacancy vacancy)
        {
            var command = new CreateVacancyCommand(vacancy);

            return _messaging.SendCommandAsync(command);

            // TODO: LWA - Should the Guid be assigned and returned here?
        }

        public Task<Vacancy> GetVacancyForEditAsync(Guid id)
        {
            return _repository.GetVacancyAsync(id);
        }

        public Task UpdateVacancyAsync(Vacancy vacancy)
        {
            var command = new UpdateVacancyCommand
            {
                Vacancy = vacancy
            };

            return _messaging.SendCommandAsync(command);
        }
    }
}