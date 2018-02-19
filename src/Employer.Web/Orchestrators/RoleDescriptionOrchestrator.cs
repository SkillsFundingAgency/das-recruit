using Esfa.Recruit.Employer.Web.ViewModels.RoleDescription;
using Esfa.Recruit.Storage.Client.Core.Commands;
using Esfa.Recruit.Storage.Client.Core.Entities.VacancyPatches;
using Esfa.Recruit.Storage.Client.Core.Messaging;
using Esfa.Recruit.Storage.Client.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class RoleDescriptionOrchestrator
    {
        private readonly IMessaging _messaging;
        private readonly IQueryVacancyRepository _queryRepository;

        public RoleDescriptionOrchestrator(IMessaging messaging, IQueryVacancyRepository queryRepository)
        {
            _messaging = messaging;
            _queryRepository = queryRepository;
        }

        public async Task<IndexViewModel> GetIndexViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _queryRepository.GetVacancyAsync(vacancyId);

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
            var patch = new RoleDescriptionPatch
            {
                Title = m.Title                
            };

            var command = new UpsertVacancyCommand
            {
                Id = m.VacancyId,
                Patch = patch
            };

            await _messaging.SendCommandAsync(command);
        }
    }
}
