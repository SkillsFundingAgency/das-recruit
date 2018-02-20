using Esfa.Recruit.Employer.Web.ViewModels.CreateVacancy;
using Esfa.Recruit.Storage.Client.Core.Commands;
using Esfa.Recruit.Storage.Client.Core.Entities.VacancyPatches;
using Esfa.Recruit.Storage.Client.Core.Messaging;
using Esfa.Recruit.Storage.Client.Core.Services;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class CreateVacancyOrchestrator
    {
        private readonly IMessaging _messaging;
        private readonly IdGenerator _idGenerator;

        public CreateVacancyOrchestrator(IMessaging messaging, IdGenerator idGenerator)
        {
            _messaging = messaging;
            _idGenerator = idGenerator;
        }

        public IndexViewModel GetIndexViewModel()
        {
            var vm = new IndexViewModel();
            return vm;
        }

        public async Task<Guid> PostIndexViewModelAsync(IndexViewModel vm)
        {
            var patch = new CreateVacancyPatch
            {
                Title = vm.Title,
                AuditVacancyCreated = DateTime.Now
            };

            var command = new UpsertVacancyCommand
            {
                Id = _idGenerator.CreateId(),
                Patch = patch
            };

            await _messaging.SendCommandAsync(command);
            
            return command.Id;
        }
    }
}
