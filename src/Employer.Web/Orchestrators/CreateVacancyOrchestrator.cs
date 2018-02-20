using Esfa.Recruit.Employer.Web.ViewModels.CreateVacancy;
using Esfa.Recruit.Storage.Client.Application.Commands;
using Esfa.Recruit.Storage.Client.Application.Services;
using Esfa.Recruit.Storage.Client.Domain.Messaging;
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
            var command = new CreateVacancyCommand();

            command.Vacancy.Title = vm.Title;

            await _messaging.SendCommandAsync(command);
            
            return command.Vacancy.Id;
        }
    }
}
