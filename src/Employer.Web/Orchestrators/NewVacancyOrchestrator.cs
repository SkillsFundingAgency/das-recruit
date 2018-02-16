using Esfa.Recruit.Employer.Web.ViewModels.NewVacancy;
using Esfa.Recruit.Storage.Client.Core.Commands;
using Esfa.Recruit.Storage.Client.Core.Messaging;
using Esfa.Recruit.Storage.Client.Core.Services;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class NewVacancyOrchestrator
    {
        private readonly IMessaging _messaging;
        private readonly IdGenerator _idGenerator;

        public NewVacancyOrchestrator(IMessaging messaging, IdGenerator idGenerator)
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
            //dummy code
            Dummy.VacancyTitle = vm.Title;

            var command = new CreateVacancyCommand
            {
                Id = _idGenerator.CreateId(),
                Title = vm.Title
            };

            await _messaging.SendCommandAsync(command);

            return command.Id;
        }
    }
}
