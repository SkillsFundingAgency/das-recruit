using Esfa.Recruit.Employer.Web.ViewModels.NewVacancy;
using Esfa.Recruit.Storage.Client.Core.Commands;
using Esfa.Recruit.Storage.Client.Core.Messaging;
using Esfa.Recruit.Storage.Client.Core.Responses;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class NewVacancyOrchestrator
    {
        private readonly IMessaging _messaging;

        public NewVacancyOrchestrator(IMessaging messaging)
        {
            _messaging = messaging;
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
                Title = vm.Title
            };

            var response = await _messaging.SendCommandAsync(command);

            return response.Id;
        }
    }
}
