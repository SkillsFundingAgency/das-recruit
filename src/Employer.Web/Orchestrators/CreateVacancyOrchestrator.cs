using Esfa.Recruit.Employer.Web.ViewModels.CreateVacancy;
using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class CreateVacancyOrchestrator
    {
        private readonly IVacancyClient _client;

        public CreateVacancyOrchestrator(IVacancyClient client)
        {
            _client = client;
        }

        public IndexViewModel GetIndexViewModel()
        {
            var vm = new IndexViewModel();
            return vm;
        }

        public async Task<Guid> PostIndexViewModelAsync(IndexViewModel vm)
        {
            var id = await _client.CreateVacancyAsync(vm.Title, vm.EmployerAccountId);
            
            return id;
        }
    }
}
