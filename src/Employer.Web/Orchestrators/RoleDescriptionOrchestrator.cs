using Esfa.Recruit.Employer.Web.ViewModels.RoleDescription;
using Esfa.Recruit.Storage.Client.Application.Commands;
using Esfa.Recruit.Storage.Client.Domain.Messaging;
using Esfa.Recruit.Storage.Client.Domain.QueryStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class RoleDescriptionOrchestrator
    {
        private readonly IMessaging _messaging;
        private readonly IQueryStoreReader _queryRepository;

        public RoleDescriptionOrchestrator(IMessaging messaging, IQueryStoreReader queryRepository)
        {
            _messaging = messaging;
            _queryRepository = queryRepository;
        }

        public async Task<IndexViewModel> GetIndexViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _queryRepository.GetVacancyForEditAsync(vacancyId);

            var vm = new IndexViewModel
            {
                CurrentVacancyTitle = vacancy.Title,
                Title = vacancy.Title
            };

            return vm;
        }

        public async Task<IndexViewModel> GetIndexViewModelAsync(IndexEditModel m)
        {
            var vm = await GetIndexViewModelAsync(m.VacancyId);

            vm.Title = m.Title;

            return vm;
        }

        public async Task PostIndexEditModelAsync(IndexEditModel m)
        {
            var vacancy = await _queryRepository.GetVacancyForEditAsync(m.VacancyId);
            
            vacancy.Title = m.Title;

            var command = new UpdateVacancyCommand
            {
                Vacancy = vacancy
            };

            await _messaging.SendCommandAsync(command);
        }
    }
}
